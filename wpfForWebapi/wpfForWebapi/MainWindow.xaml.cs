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
        #region 成员定义

        HttpClient client = new HttpClient();
        #endregion
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

        #region 登录界面事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
      
        private void loginChangePwd_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            ChangePasswordWindow.Visibility = Visibility.Visible;
        }

        private void loginetrievePwd_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            ViewModelLogin viewModelLogin = new ViewModelLogin();
            viewModelLogin.Account = cbxUserAccountLogin.Text;
            viewModelLogin.Password = pbxUserPasswordLogin.Password;
            if (cheRememberPwdLogin.IsChecked == true)
            {
                viewModelLogin.RememberPasswerd ="1";
            }
            else
            {
                viewModelLogin.RememberPasswerd = "0";
            }
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            viewModelInformation = await LoginView(viewModelLogin);
        }


        /// <summary>
        /// 登录信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> LoginView(ViewModelLogin viewModelLogin)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/Login/PostLogin", viewModelLogin);
                response.EnsureSuccessStatusCode();
                viewModelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();
                if (viewModelInformation == null)
                {
                    viewModelInformation.Message = "网络错误";
                    return viewModelInformation;
                }
                else
                {
                    return viewModelInformation;
                }
            }
            catch (HttpRequestException ex)
            {
                //后续保存到数据库里，另外再续返回到webapi的数据库里备查
                viewModelInformation.Message = ex.Message;
                return viewModelInformation;
            }
            catch (System.FormatException)
            {
                return viewModelInformation;
            }
        }

        #endregion

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
            try
            {
                #region 账号规范
                foreach (char c in tbxUserAccountRegister.Text)   //规范账号必须由字母和数字构成
                {
                    if (!(('0' <= c && c <= '9') || ('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z')))
                    {
                        throw new Exception("账号必须只由字母和数字构成！");
                    }
                }
                #endregion
                #region 密码规范
                int number = 0, character = 0;
                foreach (char c in pbxUserPasswordRegister.Password)   //规范密码必须由ASCII码33~126之间的字符构成
                {
                    if (!(33 <= c && c <= 126))
                    {
                        throw new Exception("符号错误，请重新输入！");
                    }
                    if ('0' <= c && c <= '9') //number记录数字个数
                    {
                        number++;
                    }
                    else                      //character记录字符个数
                    {
                        character++;
                    }
                }
                if (number < 5 || character < 2)  //密码的安全系数
                {
                    throw new Exception("密码安全系数太低！");
                }
                #endregion
                if (pbxUserPasswordRegister.Password == pbxSurePasswordRegister.Password)
                {
                    String UserAnswer = "1" + tbxUserAnswer1Register.Text + "2" + tbxUserAnswer2Register.Text + "3" + tbxUserAnswer3Register.Text + "4" + tbxUserAnswer4Register.Text + "5" + tbxUserAnswer5Register.Text;//拾回密码的各个答案与问题号的连接
                    ViewModelRegister viewModelRegister = new ViewModelRegister();
                    viewModelRegister.Account = tbxUserAccountRegister.Text;
                    viewModelRegister.Password = pbxUserPasswordRegister.Password;
                    viewModelRegister.SurePassword = pbxSurePasswordRegister.Password;
                    viewModelRegister.RememberPasswerd = "0";
                    viewModelRegister.RoleName = cbxUserRoleRegister.Text;
                    viewModelRegister.QuestionOrAnswer = UserAnswer;
                    ViewModelInformation viewModelInformation = new ViewModelInformation();
                    viewModelInformation = await PostView(viewModelRegister);
                }
                else
                {
                    throw new Exception("两次输入的密码不一致！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册失败！错误信息：\n" + ex.Message);
            }                    
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

        /// <summary>
        /// 注册信息提交webapi
        /// </summary>
        /// <param name="viewModelRegister">注册信息，与webapi定义一模一样</param>
        /// <returns>注册的结果，与webapi定义一模一样</returns>
        private async Task<ViewModelInformation> PostView(ViewModelRegister viewModelRegister)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/Register/PostRegister", viewModelRegister);
                response.EnsureSuccessStatusCode();
                viewModelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();
                if (viewModelInformation == null)
                {
                    viewModelInformation.Message = "网络错误";
                    return viewModelInformation;
                }
                else
                {
                    return viewModelInformation;
                }
            }
            catch (HttpRequestException ex)
            {
                //后续保存到数据库里，另外再续返回到webapi的数据库里备查
                viewModelInformation.Message = ex.Message;
                return viewModelInformation;
            }
            catch (System.FormatException)
            {
                return viewModelInformation;
            }
        }
        #endregion

        #region 修改密码界面
        private async void ChangePsw_Click(object sender, RoutedEventArgs e)
        {
            ViewModelChangePsw viewModelChangePsw = new ViewModelChangePsw();
            viewModelChangePsw.Account = tbxUserAccountChangePwd.Text;
            viewModelChangePsw.OldPassword = pbxOldPasswordChangePwd.Password;
            viewModelChangePsw.NewPassword = pbxUserPasswordChangePwd.Password;
            viewModelChangePsw.SurePassword = pbxSurePasswordChangePwd.Password;
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            viewModelInformation = await ChangePswView(viewModelChangePsw);

        }

        private void changePwdBack_click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
        }

        private async Task<ViewModelInformation> ChangePswView(ViewModelChangePsw viewModelChangePsw)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/ChangePsw/PostChangePsw", viewModelChangePsw);
                response.EnsureSuccessStatusCode();
                viewModelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();
                if (viewModelInformation == null)
                {
                    viewModelInformation.Message = "网络错误";
                    return viewModelInformation;
                }
                else
                {
                    return viewModelInformation;
                }
            }
            catch (HttpRequestException ex)
            {
                //后续保存到数据库里，另外再续返回到webapi的数据库里备查
                viewModelInformation.Message = ex.Message;
                return viewModelInformation;
            }
            catch (System.FormatException)
            {
                return viewModelInformation;
            }
        }

        #endregion


        private void UserAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        
    }
}