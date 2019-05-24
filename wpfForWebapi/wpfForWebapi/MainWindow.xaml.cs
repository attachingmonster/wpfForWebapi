using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using wpfForWebapi.ViewModels;

namespace wpfForWebapi
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 公用界面事件

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        private void Close_Click(object sender, RoutedEventArgs e)  //退出系统
        {
            SystemCommands.CloseWindow(this);
        }

        private void Move_window(object sender, MouseButtonEventArgs e)  //移动窗口
        {
            this.DragMove();
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        

        private void loginChangePwd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void loginetrievePwd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 注册账号界面



        private void loginRegister_Click(object sender, RoutedEventArgs e)    //切换界面事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            RegisterWindow.Visibility = Visibility.Visible;
            Height = 441;
        }
        private async void Registering_Click(object sender, RoutedEventArgs e)
        {
            if (tbxUserPasswordRegister.Visibility != Visibility.Collapsed)    //如果显示密码按钮事件开始，则让显示密码值赋值给隐藏密码的值
            {
                pbxUserPasswordRegister.Password = tbxUserPasswordRegister.Text;
                pbxSurePasswordRegister.Password = tbxSurePasswordRegister.Text;
            }
            String UserAnswer = "1" + tbxUserAnswer1Register.Text + "2" + tbxUserAnswer2Register.Text + "3" + tbxUserAnswer3Register.Text + "4" + tbxUserAnswer4Register.Text + "5" + tbxUserAnswer5Register.Text;//拾回密码的各个答案与问题号的连接
            ViewModelRegister viewModelRegister = new ViewModelRegister();
            viewModelRegister.Account = tbxUserAccountRegister.Text;
            viewModelRegister.Password = pbxUserPasswordRegister.Password;
            viewModelRegister.RememberPasswerd = pbxSurePasswordRegister.Password;
            viewModelRegister.RememberPasswerd = "0";
            viewModelRegister.RoleName = cbxUserRoleRegister.Text;
            viewModelRegister.Answer = UserAnswer;
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            viewModelInformation= await PostView(viewModelRegister);
      
        }
        private void registerBack1_Click(object sender, RoutedEventArgs e)   //返回事件
        {
            RegisterWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
            Height = 311;
            tbxUserAccountRegister.Text = "";
            pbxUserPasswordRegister.Password = "";
            pbxSurePasswordRegister.Password = "";
            tbxUserAnswer1Register.Text = "";
            tbxUserAnswer2Register.Text = "";
            tbxUserAnswer3Register.Text = "";
            tbxUserAnswer4Register.Text = "";
            tbxUserAnswer5Register.Text = "";
            tbxUserPasswordRegister.Text = "";
            tbxSurePasswordRegister.Text = "";

        }
        /*private void registerBack2_Click(object sender, RoutedEventArgs e)
        {
            /var viewModels = (from us in unitOfWork.SysUserRepository.Get()
                              join ur in unitOfWork.SysUserRoleRepository.Get() on us.ID equals ur.SysUserID
                              join r in unitOfWork.SysRoleRepository.Get() on ur.SysRoleID equals r.ID

                              select new ViewModel { ViewUserAccount = us.UserAccount, ViewRoleName = r.RoleName, ViewRoleDec = r.RoleDec }).ToList();
            ListView.ItemsSource = viewModels;
            ListView.SelectedIndex = 0;
            ListView.Items.Refresh();
            LabTextListView.Content = cbxUserAccountLogin.Text + "用户为管理员";
            RegisterWindow.Visibility = Visibility.Collapsed;
            ListViewWindow.Visibility = Visibility.Visible;
            Height = 421;
            Width = 656;
            tbxUserAccountRegister.Text = "";
            pbxUserPasswordRegister.Password = "";
            pbxSurePasswordRegister.Password = "";
            tbxUserAnswer1Register.Text = "";
            tbxUserAnswer2Register.Text = "";
            tbxUserAnswer3Register.Text = "";
            tbxUserAnswer4Register.Text = "";
            tbxUserAnswer5Register.Text = "";
            tbxUserPasswordRegister.Text = "";
            tbxSurePasswordRegister.Text = "";
        }*/
        private void registerShowPassword_Check(object sender, RoutedEventArgs e) //注册界面中显示密码事件
        {
            tbxUserPasswordRegister.Visibility = Visibility.Visible;
            pbxUserPasswordRegister.Visibility = Visibility.Collapsed;
            tbxUserPasswordRegister.Text = pbxUserPasswordRegister.Password;

            tbxSurePasswordRegister.Visibility = Visibility.Visible;
            pbxSurePasswordRegister.Visibility = Visibility.Collapsed;
            tbxSurePasswordRegister.Text = pbxSurePasswordRegister.Password;
        }
        private void registerHiddenPassword_Check(object sender, RoutedEventArgs e) //注册界面中隐藏密码事件
        {
            tbxUserPasswordRegister.Visibility = Visibility.Collapsed;
            pbxUserPasswordRegister.Visibility = Visibility.Visible;
            pbxUserPasswordRegister.Password = tbxUserPasswordRegister.Text;

            tbxSurePasswordRegister.Visibility = Visibility.Collapsed;
            pbxSurePasswordRegister.Visibility = Visibility.Visible;
            pbxSurePasswordRegister.Password = tbxSurePasswordRegister.Text;
        }

        #endregion
        private async Task<ViewModelInformation> PostView(ViewModelRegister viewModelRegister)
        {
            ViewModelInformation modelInformation = null;
            //将用户发送给api服务器，判断是否登录成功
            try
            {

                //Post提交数据时直接把要提交的数据格式要和Webapi的接收格式一致。不需要再转换了
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/Register/PostRegister", viewModelRegister);
                response.EnsureSuccessStatusCode();
                 modelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();//MsgDTO需和WebApi的MSGHelper这个类保持一致
                if (modelInformation== null)
                {
                    modelInformation.message = "网络错误";
                    return modelInformation;
                }       
                else
                {
                    return modelInformation;
                }
            }
            catch (HttpRequestException ex)
            {
                modelInformation.message = ex.Message;
                return modelInformation;
            }
            catch (System.FormatException)
            {
                return modelInformation;
            }
        }

        private void UserAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}