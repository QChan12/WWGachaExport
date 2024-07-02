using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WWGachaExport.Models;

namespace WWGachaExport.Controls
{
    /// <summary>
    /// GachaPoolControl.xaml 的交互逻辑
    /// </summary>
    public partial class GachaPoolControl : UserControl
    {
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(GameUser), typeof(GachaPoolControl));

        public GameUser User
        {
            get { return (GameUser)GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }

        public GachaPoolControl()
        {
            InitializeComponent();
        }

    }
}
