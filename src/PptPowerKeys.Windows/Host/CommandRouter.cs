using System;
using System.Collections.Generic;
using System.Linq;
using PptPowerKeys.Core.Commands;
using PptPowerKeys.Core.Colors;
using PptPowerKeys.Core.Geometry;
using PptPowerKeys.Core.Layout;
using PptPowerKeys.Core.Text;
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
    /// S09-003: Group / Ungroup / Z-order HostScript commands (6 COM parity commands).
    /// S09-004: Multi-slide paste / remove shape HostScript commands (2 COM parity commands).
    /// S09-005: Format color HostScript commands (Fill/Line/Text + toggle black/white).
    /// S09-006: Text HostScript commands (paste plain, ellipsis, scripts, addup).
    /// S10-001: Slide HostScript commands (CopySlide, MoveSlidesToBackup).
    /// S10-002: View/print HostScript commands (zoom, sorter, slideshow, grid, guides, print).
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

            if (GroupZOrderCommands.IsGroupZOrderCommand(command))
            {
                return ExecuteGroupZOrder(command);
            }

            if (MultiSlideShapeCommands.IsMultiSlideShapeCommand(command))
            {
                return ExecuteMultiSlideShape(command);
            }

            if (FormatColorCommands.IsFormatColorCommand(command))
            {
                return ExecuteFormatColor(command);
            }

            if (TextCommands.IsTextCommand(command))
            {
                return ExecuteText(command);
            }

            if (SlideCommands.IsSlideCommand(command))
            {
                return ExecuteSlide(command);
            }

            if (ViewPrintCommands.IsViewPrintCommand(command))
            {
                return ExecuteViewPrint(command);
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

        private CommandExecutionResult ExecuteGroupZOrder(CommandIds command)
        {
            switch (command)
            {
                case CommandIds.Group:
                {
                    int count = _host.GroupSelectedShapes();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Grouped {count} shape(s).",
                    };
                }

                case CommandIds.Ungroup:
                    _host.UngroupSelectedShape();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = "Ungrouped.",
                    };

                case CommandIds.BringToFront:
                case CommandIds.SendToBack:
                case CommandIds.BringForward:
                case CommandIds.SendBackward:
                    _host.ApplyZOrderToSelection(command);
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = GetZOrderSuccessMessage(command),
                    };

                default:
                    throw new InvalidOperationException($"Unknown group/z-order command: {command}.");
            }
        }

        private static string GetZOrderSuccessMessage(CommandIds command) =>
            command switch
            {
                CommandIds.BringToFront => "Brought to front.",
                CommandIds.SendToBack => "Sent to back.",
                CommandIds.BringForward => "Brought forward.",
                CommandIds.SendBackward => "Sent backward.",
                _ => throw new InvalidOperationException($"Unknown z-order command: {command}."),
            };

        private CommandExecutionResult ExecuteMultiSlideShape(CommandIds command)
        {
            switch (command)
            {
                case CommandIds.PasteShapeToSelectedSlides:
                {
                    int count = _host.PasteShapeToSelectedSlides();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Pasted shape to {count} slide(s).",
                    };
                }

                case CommandIds.RemoveShapeFromSelectedSlides:
                {
                    var (slidesProcessed, shapesRemoved) = _host.RemoveShapeFromSelectedSlides();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Removed {shapesRemoved} shape(s) from {slidesProcessed} slide(s).",
                    };
                }

                default:
                    throw new InvalidOperationException($"Unknown multi-slide shape command: {command}.");
            }
        }

        private CommandExecutionResult ExecuteFormatColor(CommandIds command)
        {
            if (command == CommandIds.ToggleFillBlackWhite)
            {
                int toggled = _host.ToggleFillBlackWhite();
                return new CommandExecutionResult
                {
                    Changed = true,
                    Message = $"Toggled fill on {toggled} shape(s).",
                };
            }

            var shapes = _host.ReadSelectedShapeBounds();
            if (shapes.Count == 0)
            {
                throw new InvalidOperationException("Select one or more shapes first.");
            }

            var theme = _host.ReadPresentationThemeColors();
            var recent = _settingsStore.GetRecentColors();
            var palette = ColorPaletteBuilder.Build(
                theme,
                recent,
                DefaultColorPalette.FallbackTheme);

            var shapeIds = shapes.Select(shape => shape.Id).ToList();
            string color = FormatColorCycleStore.NextPaletteColor(command, palette, shapeIds);

            int count = command switch
            {
                CommandIds.FillColor => _host.ApplyFillColor(color),
                CommandIds.LineColor => _host.ApplyLineColor(color),
                CommandIds.TextColor => _host.ApplyTextColor(color),
                _ => throw new InvalidOperationException($"Unknown palette color command: {command}."),
            };

            _settingsStore.RecordRecentColor(color);
            string label = command switch
            {
                CommandIds.FillColor => "Fill",
                CommandIds.LineColor => "Line",
                CommandIds.TextColor => "Text",
                _ => "Color",
            };

            return new CommandExecutionResult
            {
                Changed = true,
                Message = $"{label} color {color} applied to {count} shape(s).",
            };
        }

        private CommandExecutionResult ExecuteText(CommandIds command)
        {
            switch (command)
            {
                case CommandIds.PasteUnformatted:
                {
                    int count = _host.PasteUnformattedText();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Pasted plain text into {count} shape(s).",
                    };
                }

                case CommandIds.ReplaceWithEllipsis:
                {
                    int count = _host.ReplaceSelectedTextWithEllipsis();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Replaced text with \"...\" on {count} shape(s).",
                    };
                }

                case CommandIds.ToggleSuperscript:
                {
                    int count = _host.ToggleSuperscript();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Toggled superscript on {count} shape(s).",
                    };
                }

                case CommandIds.ToggleSubscript:
                {
                    int count = _host.ToggleSubscript();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Toggled subscript on {count} shape(s).",
                    };
                }

                case CommandIds.AddupTextFields:
                {
                    var texts = _host.ReadSelectedShapeTexts();
                    var stats = NumberAggregator.Compute(texts);
                    string mode = _settingsStore.Current.AddupDisplayMode;
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = AddupStatusFormatter.Format(stats, mode),
                    };
                }

                default:
                    throw new InvalidOperationException($"Unknown text command: {command}.");
            }
        }

        private CommandExecutionResult ExecuteSlide(CommandIds command)
        {
            switch (command)
            {
                case CommandIds.CopySlide:
                    _host.DuplicateSelectedSlide();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = "Slide duplicated.",
                    };

                case CommandIds.MoveSlidesToBackup:
                {
                    int count = _host.MoveSelectedSlidesToBackup();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = $"Moved {count} slide(s) to backup (end of deck).",
                    };
                }

                default:
                    throw new InvalidOperationException($"Unknown slide command: {command}.");
            }
        }

        private CommandExecutionResult ExecuteViewPrint(CommandIds command)
        {
            switch (command)
            {
                case CommandIds.ToggleZoom:
                {
                    bool fit = _host.ToggleZoomFit();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = fit ? "Zoom set to fit." : "Zoom set to 100%.",
                    };
                }

                case CommandIds.ToggleSlideSorter:
                {
                    bool sorter = _host.ToggleSlideSorterView();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = sorter
                            ? "Switched to slide sorter."
                            : "Switched to normal view.",
                    };
                }

                case CommandIds.StartSlideShow:
                    _host.StartSlideShowFromCurrentSlide();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = "Slide show started.",
                    };

                case CommandIds.ToggleGrid:
                {
                    bool on = _host.ToggleGridLines();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = on ? "Grid lines on." : "Grid lines off.",
                    };
                }

                case CommandIds.ToggleGuides:
                {
                    bool on = _host.ToggleGuides();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = on ? "Guides on." : "Guides off.",
                    };
                }

                case CommandIds.PrintSlide:
                    _host.PrintCurrentSlide();
                    return new CommandExecutionResult
                    {
                        Changed = true,
                        Message = "Printing current slide.",
                    };

                default:
                    throw new InvalidOperationException($"Unknown view/print command: {command}.");
            }
        }
    }
}
