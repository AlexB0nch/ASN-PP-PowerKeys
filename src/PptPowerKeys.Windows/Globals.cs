namespace PptPowerKeys.Windows
{
    internal sealed partial class Globals
    {
        private Globals()
        {
        }

        private static ThisAddIn _thisAddIn;

        private static global::Microsoft.Office.Tools.Factory _factory;

        internal static ThisAddIn ThisAddIn
        {
            get
            {
                return _thisAddIn;
            }
            set
            {
                if (_thisAddIn == null)
                {
                    _thisAddIn = value;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }

        internal static global::Microsoft.Office.Tools.Factory Factory
        {
            get
            {
                return _factory;
            }
            set
            {
                if (_factory == null)
                {
                    _factory = value;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
    }
}
