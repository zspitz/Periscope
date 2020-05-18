using System.Windows;
using System.Windows.Controls.Primitives;

namespace Periscope {
    public partial class VisualizerWindowChrome {
        public VisualizerWindowChrome() {
            InitializeComponent();

            Loaded += (s, e) => {
                optionsLink.Click += (s, e) => optionsPopup.IsOpen = true;

                // https://stackoverflow.com/a/21436273/111794
                optionsPopup.CustomPopupPlacementCallback += (popupSize, targetSize, offset) => {
                    return new[] {
                        new CustomPopupPlacement() {
                            Point = new Point(targetSize.Width - popupSize.Width, targetSize.Height)
                        }
                    };
                };
            };

        }
    }
}
