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
using wpfForWebapi.DAL;
using wpfForWebapi.Methods;
using wpfForWebapi.Model;
using wpfForWebapi.ViewModels;

namespace wpfForWebapi
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 成员定义
        AccountContext db = new AccountContext();//数据库上下文实例
        UnitOfWork unitOfWork = new UnitOfWork();//单元工厂实例
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        

      

        private void LoginetrievePwd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

        }

        #region 注册账号界面



        private void LoginRegister_Click(object sender, RoutedEventArgs e)    //切换注册界面与登陆界面事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            RegisterWindow.Visibility = Visibility.Visible;
            Height = 441;
        }
        private async void Registering_Click(object sender, RoutedEventArgs e)
        {
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            ViewModelRegister viewModelRegister = new ViewModelRegister();
            try
            {

                if (tbxUserPasswordRegister.Visibility != Visibility.Collapsed)    //如果显示密码按钮事件开始，则让显示密码值赋值给隐藏密码的值
                {
                    pbxUserPasswordRegister.Password = tbxUserPasswordRegister.Text;
                    pbxSurePasswordRegister.Password = tbxSurePasswordRegister.Text;
                }
                if (tbxUserAccountRegister.Text != "")    //判断用户账号是否为空
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
                    if (pbxUserPasswordRegister.Password != "")    //判断密码是否为空
                    {
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
                        if (pbxSurePasswordRegister.Password.Equals(pbxUserPasswordRegister.Password))    //判断密码与确认密码是否相等
                        {
                            string UserAnswer = "1" + tbxUserAnswer1Register.Text + "2" + tbxUserAnswer2Register.Text + "3" + tbxUserAnswer3Register.Text + "4" + tbxUserAnswer4Register.Text + "5" + tbxUserAnswer5Register.Text;//拾回密码的各个答案与问题号的连接
                            if (UserAnswer != "1" + "2" + "3" + "4" + "5")    //判断拾回密码是否为空
                            {
                                //对需要Post的ViewModelRegister进行赋值
                                
                                viewModelRegister.Account = tbxUserAccountRegister.Text;
                                viewModelRegister.Password = CreateMD5.EncryptWithMD5(pbxUserPasswordRegister.Password);
                                viewModelRegister.QuestionOrAnswer = CreateMD5.EncryptWithMD5(UserAnswer);
                                viewModelRegister.RoleName = cbxUserRoleRegister.Text;

                                viewModelInformation = await PostViewRegister(viewModelRegister);
                               
                                    throw new Exception(viewModelInformation.Message);
                                
                            }
                            else
                            {
                                throw new Exception("密码拾回问题答案不能为空！");
                            }
                        }
                        else
                        {
                            throw new Exception("两次输入的密码不一致！");
                        }
                    }
                    else
                    {
                        throw new Exception("密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception("用户账号不能为空！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally {
                if ("注册成功！".Equals(viewModelInformation.Message))
                {
                    var CurrentData = new Data();
                    CurrentData.UserAccount = viewModelRegister.Account;
                    CurrentData.UserPassword = viewModelRegister.Password;
                    CurrentData.QuestionOrAnswer = viewModelRegister.QuestionOrAnswer;
                    CurrentData.RememberPassword = "0";
                    CurrentData.RoleName = viewModelRegister.RoleName;
                    unitOfWork.DataRepository.Insert(CurrentData);    //增加新SysUser
                    unitOfWork.Save();
                }
            }
        }
        private void registerBack1_Click(object sender, RoutedEventArgs e)   //Back切换注册界面与登陆界面事件
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
        private void registerShowPassword_Check(object sender, RoutedEventArgs e) //注册界面中密码框显示密码事件
        {
            tbxUserPasswordRegister.Visibility = Visibility.Visible;
            pbxUserPasswordRegister.Visibility = Visibility.Collapsed;
            tbxUserPasswordRegister.Text = pbxUserPasswordRegister.Password;

            tbxSurePasswordRegister.Visibility = Visibility.Visible;
            pbxSurePasswordRegister.Visibility = Visibility.Collapsed;
            tbxSurePasswordRegister.Text = pbxSurePasswordRegister.Password;
        }
        private void registerHiddenPassword_Check(object sender, RoutedEventArgs e) //注册界面中密码框隐藏密码事件
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
        private async Task<ViewModelInformation> PostViewRegister(ViewModelRegister viewModelRegister)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation  = new ViewModelInformation();
            try
            {
                
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/Register/PostRegister", viewModelRegister);
                response.EnsureSuccessStatusCode();
                 viewModelInformation = await response.Content.ReadAsAsync<ViewModelInformation>();
                if (viewModelInformation== null)
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


        private void LoginChangePwd_Click(object sender, RoutedEventArgs e)    //切换修改密码界面与登陆界面事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            ChangePasswordWindow.Visibility = Visibility.Visible;
        }
        private async void ChangePsw_Click(object sender, RoutedEventArgs e)
        {
           
            ViewModelInformation vMInformation = new ViewModelInformation();
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            ViewModelChangePwd viewModelChangePwd = new ViewModelChangePwd();
            try
            {

                string OldPassword = CreateMD5.EncryptWithMD5(pbxOldPasswordChangePwd.Password);     //原密码
                string NewPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordChangePwd.Password);     //新密码
                //传送需要判断的账号和密码
                

                viewModelChangePwd.OldPassword = OldPassword;
                viewModelChangePwd.Account = tbxUserAccountChangePwd.Text;

                vMInformation = await PostViewChangePwdModel(viewModelChangePwd);
                if ("判断正确".Equals(vMInformation.Message))
                {
                    if (pbxUserPasswordChangePwd.Password != "")
                    {
                        if (!pbxOldPasswordChangePwd.Password.Equals(pbxUserPasswordChangePwd.Password))
                        {
                            int number = 0, character = 0;
                            foreach (char c in pbxUserPasswordChangePwd.Password)   //规范密码必须由ASCII码33~126之间的字符构成
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
                                throw new Exception("新密码安全系数太低！");
                            }
                            if (pbxSurePasswordChangePwd.Password.Equals(pbxUserPasswordChangePwd.Password)) //新密码与确认密码是否相等
                            {
                                viewModelChangePwd.NewPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordChangePwd.Password);
                                
                                viewModelInformation = await PostViewChangePwd(viewModelChangePwd);
                             
                                    throw new Exception(viewModelInformation.Message);
                                
                            }
                            else
                            {
                                throw new Exception("两次输入的密码不一致！");
                            }
                        }
                        else
                        {
                            throw new Exception("新密码与原密码不能相同！");
                        }
                    }
                    else
                    {
                        throw new Exception("新密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception(vMInformation.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally {
                if ("修改密码成功".Equals(viewModelInformation.Message))
                {
                    var data = unitOfWork.DataRepository.Get().Where(s => s.UserAccount.Equals(viewModelChangePwd.Account)).FirstOrDefault();
                    data.UserPassword = viewModelChangePwd.NewPassword;
                    unitOfWork.DataRepository.Update(data);//更改密码
                    unitOfWork.Save();
                }
            }
        }
        /// <summary>
        /// 修改密码界面的用户账号和原密码提交webapi
        /// </summary>
        /// <param name="viewModelChangePwd">判断所需的账号密码信息</param>
        /// <returns>判断后的结果，与webapi定义一模一样</returns>
        private async Task<ViewModelInformation> PostViewChangePwdModel(ViewModelChangePwd viewModelChangePwd)
        {
            ViewModelInformation viewModelInformation = null;
            //异常中断，程序不会破溃          
            try
            {
                //Post异步提交信息，格式为Json

                var response = await client.PostAsJsonAsync("http://localhost:60033/api/ChangePwdModel/PostChangePwdModel", viewModelChangePwd);
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
        /// <summary>
        /// 修改密码信息提交webapi
        /// </summary>
        /// <param name="viewModelRegister">修改密码信息，与webapi定义一模一样</param>
        /// <returns>修改密码的结果，与webapi定义一模一样</returns>
        private async Task<ViewModelInformation> PostViewChangePwd(ViewModelChangePwd viewModelChangePwd)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/ChangePwd/PostChangePwd", viewModelChangePwd);
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
        private void ChangePwdBack_click(object sender, RoutedEventArgs e)      //返回//切换修改密码界面与登陆界面事件
        {
            LoginWindow.Visibility = Visibility.Visible;
            ChangePasswordWindow.Visibility = Visibility.Collapsed;
            tbxUserAccountChangePwd.Text = "";
            pbxOldPasswordChangePwd.Password = "";
            pbxUserPasswordChangePwd.Password = "";
            pbxSurePasswordChangePwd.Password = "";
        }
        #endregion
        private void UserAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

       
    }
}
