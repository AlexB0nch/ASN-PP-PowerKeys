using System;
using System.Collections.Generic;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Core.Layout;
using PptPowerKeys.Windows.Settings;

namespace PptPowerKeys.Windows.Host
{
    /// <summary>
    /// Routes <see cref="CommandIds"/> to in-process Core (ServerLayout) or host scripts.
    /// S08-001: all 32 <see cref="LayoutEngine.IsLayoutCommand"/> ids via <see cref="ExecuteServerLayout"/>.
    /// S08-002: passes <see cref="LayoutOptions.SnapToGrid"/> from <see cref="WindowsUserSettingsStore"/>.
    /// S08-004: Copy-and-align HostScript commands (duplicate + layout).
    /// S08-005: Position clipboard HostScript commands (Copy/Paste object position).
    /// S09-001: Insert-shape HostScript commands (rectangle, square, ellipse, line, textbox, arrow).
    /// S09-002: Smart-duplicate HostScript commands (DuplicateRight/Left/Up/Down + gap memory).
    /// </summary>
    public sealed class CommandRouter
    {
        private readonly IComHostAdapter _host;
        private readonly WindowsUserSettingsStore _settingsStore;

        public CommandRouter(IComHostAdapter host, WindowsUserSettingsStore settingsStore)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _settingsStore = settingsStore ?? throw new ArgumentNullException(nameof(settingsStore));
        }

        public CommandExecutionResult Execute(CommandIds command)
        {
            if (LayoutEngine.IsLayoutCommand(command))
            {
                return CommandExecutionResult.FromLayoutResult(ExecuteServerLayout(command));
            }

            if (CopyAndAlignCommands.IsCopyAndAlign(command))
            {
                return ExecuteCopyAndAlign(command);
            }

            if (PositionCommands.IsPositionCommand(command))
            {
                return command switch
                {
                    CommandIds.CopyObjectPosition => ExecuteCopyObjectPosition(),
                    CommandIds.PasteObjectPosition => ExecutePasteObjectPosition(),
                    _ => throw new InvalidOperationException($"Unknown position command: {command}."),
                };
            }

            if (InsertShapeCommands.IsInsertShape(command))
            {
                return ExecuteInsertShape(command);
            }

            if (DuplicateCommands.IsDuplicateCommand(command))
            {
                return ExecuteDuplicate(command);
            }

            throw new NotSupportedException(
                $"Command '{command}' is not implemented in PptPowerKeys.Windows yet.");
        }

        private LayoutOptions GetLayoutOptions() =>
            new LayoutOptions { SnapToGrid = _settingsStore.Current.SnapToGrid };

        private LayoutResult ExecuteServerLayout(CommandIds command)
        {
            var shapes = _host.ReadSelectedShapeBounds();
            var request = new LayoutRequest
            {
                Command = command,
                Shapes = shapes,
                Options = GetLayoutOptions(),
            };

            var result = LayoutEngine.Apply(request);
            if (result.Changed)
            {
                _host.ApplyShapeBounds(result.Shapes);
            }

            return result;
        }

        private CommandExecutionResult ExecuteCopyAndAlign(CommandIds command)
        {
            if (!CopyAndAlignCommands.TryMapToLayoutCommand(command, out var layoutCommand))
            {
                throw new InvalidOperationException($"Unknown copy-and-align command: {command}.");
            }

            var originals = _host.ReadSelectedShapeBounds();
            if (originals.Count == 0)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            var clones = _host.CloneSelectedAtSourcePositions();
            if (clones.Count == 0)
            {
                throw new InvalidOperationException("Could not duplicate the selected shapes.");
            }

            var combined = new List<ShapeBounds>(originals.Count + clones.Count);
            combined.AddRange(originals);
            combined.AddRange(clones);

            var request = new LayoutRequest
            {
                Command = layoutCommand,
                Shapes = combined,
                AnchorIndex = originals.Count - 1,
                Options = GetLayoutOptions(),
            };

            var result = LayoutEngine.Apply(request);
            if (!result.Changed)
            {
                return new CommandExecutionResult
                {
                    Changed = false,
                    Message = result.Message ?? "Nothing to change.",
                };
            }

            _host.ApplyShapeBoundsOnSlide(result.Shapes);
            return new CommandExecutionResult
            {
                Changed = true,
                Message = $"Duplicated and aligned {clones.Count} shape(s).",
            };
        }

        private CommandExecutionResult ExecuteCopyObjectPosition()
        {
            var shapes = _host.ReadSelectedShapeBounds();
            if (shapes.Count == 0)
            {
                throw new InvalidOperationException("Select a shape first.");
            }

            var anchor = shapes[shapes.Count - 1];
            PositionClipboardStore.Set(anchor.Left, anchor.Top);
            return new CommandExecutionResult
            {
                Changed = true,
                Message = $"Copied position ({anchor.Left:F1}, {anchor.Top:F1}).",
            };
        }

        private CommandExecutionResult ExecutePasteObjectPosition()
        {
            var snapshot = PositionClipboardStore.Get();
            if (snapshot == null)
            {
                throw new InvalidOperationException("Copy a position first (Copy object position).");
            }

            var count = _host.ApplyPositionToSelection(snapshot.Value.Left, snapshot.Value.Top);
            if (count == 0)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            return new CommandExecutionResult
            {
                Changed = true,
                Message = $"Pasted position to {count} shape(s).",
            };
        }

        private CommandExecutionResult ExecuteInsertShape(CommandIds command)
        {
            _host.InsertShape(command);
            return new CommandExecutionResult
            {
                Changed = true,
                Message = GetInsertShapeSuccessMessage(command),
            };
        }

        private static string GetInsertShapeSuccessMessage(CommandIds command) =>
            command switch
            {
                CommandIds.InsertRectangle or CommandIds.InsertSquare => "Rectangle inserted.",
                CommandIds.InsertEllipse => "Ellipse inserted.",
                CommandIds.InsertLine or CommandIds.InsertArrow => "Line inserted.",
                CommandIds.InsertTextbox => "Text box inserted.",
                _ => throw new InvalidOperationException($"Unknown insert-shape command: {command}."),
            };

        private CommandExecutionResult ExecuteDuplicate(CommandIds command)
        {
            var sources = _host.ReadSelectedShapeBounds();
            if (sources.Count == 0)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            double gap = DuplicateGapStore.GetGap(command);
            var targets = new List<ShapeBounds>(sources.Count);
            foreach (var source in sources)
            {
                var target = DuplicationEngine.ComputeDuplicate(command, source, gap);
                if (target == null)
                {
                    throw new InvalidOperationException($"Unknown duplicate command: {command}.");
                }

                targets.Add(target.Value);
            }

            int count = _host.DuplicateSelectedAtPositions(sources, targets);
            if (count == 0)
            {
                throw new InvalidOperationException("Could not duplicate the selected shapes.");
            }

            DuplicateGapStore.SetGap(command, gap);
            string gapNote = gap > 0 ? $" (gap {gap} pt)" : string.Empty;
            return new CommandExecutionResult
            {
                Changed = true,
                Message = $"Duplicated {count} shape(s){gapNote}.",
            };
        }
    }
}
