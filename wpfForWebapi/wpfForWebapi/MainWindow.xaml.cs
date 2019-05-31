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
            var data = unitOfWork.DataRepository.Get();
        }

        #region 公用界面事件

        private void Min_Click(object sender, RoutedEventArgs e)//缩小窗口
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
        private void Window_Loaded(object sender, RoutedEventArgs e)//窗口初始化事件
        {
            var user = unitOfWork.DataRepository.Get();         //combobox的更新
            cbxUserAccountLogin.ItemsSource = user.ToList();       //combobox数据源连接数据库
            cbxUserAccountLogin.DisplayMemberPath = "UserAccount";  //combobox下拉显示的值
            cbxUserAccountLogin.SelectedValuePath = "UserAccount";  //combobox选中项显示的值
            cbxUserAccountLogin.SelectedIndex = 0;               //登陆界面 combobox初始显示第一项
            var u = user.Where(s => s.UserAccount.Equals(cbxUserAccountLogin.Text)).FirstOrDefault();
            if (u != null)
            {
                if (u.RememberPassword == "1")              //判断该对象的 记住密码 是否为 已选
                {
                    pbxUserPasswordLogin.Password = CreateMD5.EncryptWithMD5(u.UserPassword);//给passwordbox一串固定密码
                    cheRememberPwdLogin.IsChecked = true;     //让记住密码选择框显示选中
                }
            }
        }


        private void loginChangePwd_Click(object sender, RoutedEventArgs e)//登录界面的修改密码按钮事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            ChangePasswordWindow.Visibility = Visibility.Visible;
        }

        private void loginetrievePwd_Click(object sender, RoutedEventArgs e)//登录界面的找回密码按钮事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            RetrievePasswordWindow.Visibility = Visibility.Visible;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)//登录按钮事件
        {
            try
            {
                if (cbxUserAccountLogin.Text != "")//判断账号是否为空
                {
                    if (pbxUserPasswordLogin.Password != "")//判断密码是否为空
                    {
                        ViewModelLogin viewModelLogin = new ViewModelLogin();
                        viewModelLogin.Account = cbxUserAccountLogin.Text;
                        viewModelLogin.Password = CreateMD5.EncryptWithMD5(pbxUserPasswordLogin.Password);
                        if (cheRememberPwdLogin.IsChecked == true)
                        {
                            viewModelLogin.RememberPassword = "1";
                        }
                        else
                        {
                            viewModelLogin.RememberPassword = "0";
                        }
                        //传输登录信息到webapi
                        ViewModelInformation viewModelInformation = new ViewModelInformation();
                        viewModelInformation = await LoginView(viewModelLogin);
                        MessageBox.Show(viewModelInformation.Message);
                        //更新本地数据库中对应账号对应记住密码的情况
                        var user = unitOfWork.DataRepository.Get().Where(s => s.UserAccount.Equals(cbxUserAccountLogin.Text)).FirstOrDefault();
                        if (user != null)
                        {
                            user.RememberPassword = viewModelLogin.RememberPassword;
                            unitOfWork.Save();
                        }
                        //根据远端数据库返回的账号对应的角色信息进入不同窗口
                        if (viewModelInformation.Message == "登录成功")
                        {
                            ViewModelLogin viewModelLogin2 = new ViewModelLogin();
                            viewModelLogin2.Account = cbxUserAccountLogin.Text;
                            ViewModelInformation viewModelInformation2 = new ViewModelInformation();
                            viewModelInformation2 = await UserRole(viewModelLogin);
                            if (viewModelInformation2.Message == "admin")
                            {
                                LoginWindow.Visibility = Visibility.Collapsed;
                                AdminWindow.Visibility = Visibility.Visible;
                                Height = 421;
                                Width = 656;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception("账号不能为空！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录失败！错误信息：\n" + ex.Message);
            }

        }


        /// <summary>
        /// 用户角色信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> UserRole(ViewModelLogin viewModelLogin)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/UserRole/PostUserRole", viewModelLogin);
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

        Dictionary<string, string> dic = new Dictionary<string, string>();

        private void loginRegister_Click(object sender, RoutedEventArgs e)    //切换界面事件
        {
            LoginWindow.Visibility = Visibility.Collapsed;
            RegisterWindow.Visibility = Visibility.Visible;
            Height = 441;
        }
        private async void Registering_Click(object sender, RoutedEventArgs e)//注册账号事件
        {
            if (tbxUserPasswordRegister.Visibility != Visibility.Collapsed)    //如果显示密码按钮事件开始，则让显示密码值赋值给隐藏密码的值
            {
                pbxUserPasswordRegister.Password = tbxUserPasswordRegister.Text;
                pbxSurePasswordRegister.Password = tbxSurePasswordRegister.Text;
            }
            try
            {
                if (tbxUserAccountRegister.Text != "")//判断账号是否为空
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
                    if (pbxUserPasswordRegister.Password != "")//判断密码是否为空
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
                        if (tbxUserAnswerRegister.Text != "")//判断密码拾回问题答案是否为空
                        {
                            if (pbxUserPasswordRegister.Password == pbxSurePasswordRegister.Password)//判断密码和确认密码是否相同
                            {
                                String UserAnswer = cbxRegister.Text + tbxUserAnswerRegister.Text;
                                ViewModelRegister viewModelRegister = new ViewModelRegister();
                                viewModelRegister.Account = tbxUserAccountRegister.Text;
                                viewModelRegister.Password = CreateMD5.EncryptWithMD5(pbxUserPasswordRegister.Password);
                                viewModelRegister.SurePassword = CreateMD5.EncryptWithMD5(pbxSurePasswordRegister.Password);
                                viewModelRegister.RememberPassword = "0";
                                viewModelRegister.RoleName = cbxUserRoleRegister.Text;
                                viewModelRegister.QuestionOrAnswer = CreateMD5.EncryptWithMD5(UserAnswer);
                                ViewModelInformation viewModelInformation = new ViewModelInformation();
                                //注册信息提交到webapi
                                viewModelInformation = await PostView(viewModelRegister);
                                MessageBox.Show(viewModelInformation.Message);
                                if (viewModelInformation.Message == "注册成功")
                                {
                                    //往本地数据库里插入注册信息
                                    var CurrentData = new Data();
                                    CurrentData.UserAccount = tbxUserAccountRegister.Text;
                                    CurrentData.UserPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordRegister.Password);
                                    CurrentData.QuestionOrAnswer = CreateMD5.EncryptWithMD5(UserAnswer);
                                    CurrentData.RememberPassword = "0";
                                    CurrentData.RoleName = cbxUserRoleRegister.Text;
                                    unitOfWork.DataRepository.Insert(CurrentData);    //增加新SysUser
                                    unitOfWork.Save();
                                }
                                //combobox绑定数据源
                                var user = unitOfWork.DataRepository.Get();         //combobox的更新
                                cbxUserAccountLogin.ItemsSource = user.ToList();       //combobox数据源连接数据库
                                cbxUserAccountLogin.DisplayMemberPath = "UserAccount";  //combobox下拉显示的值
                                cbxUserAccountLogin.SelectedValuePath = "UserAccount";  //combobox选中项显示的值
                                cbxUserAccountLogin.SelectedIndex = 0;               //登陆界面 combobox初始显示第一项
                            }
                            else
                            {
                                throw new Exception("两次输入的密码不一致！");
                            }
                        }
                        else
                        {
                            throw new Exception("密码拾回问题答案不能为空！");
                        }
                    }
                    else
                    {
                        throw new Exception("密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception("账号不能为空！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册失败！错误信息：\n" + ex.Message);
            }
        }

        private void CbxRegister_Loaded(object sender, RoutedEventArgs e)//注册界面的combobox的初始化下拉菜单事件
        {
            dic.Add("你最喜欢的颜色是", "1");
            dic.Add("你的生日是", "2");
            dic.Add("你的父亲叫什么名字", "3");
            dic.Add("你最喜欢做什么", "4");
            dic.Add("你的梦想是", "5");
            cbxRegister.ItemsSource = dic;
            cbxRegister.DisplayMemberPath = "Key";
            cbxRegister.SelectedIndex = 0;
        }

        private void CbxRegister_SelectionChanged(object sender, SelectionChangedEventArgs e)//清空密保问题的答案事件
        {
            tbxUserAnswerRegister.Text = "";
        }

        private void registerBack1_Click(object sender, RoutedEventArgs e)   //注册界面的返回事件
        {
            RegisterWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
            Height = 311;
            tbxUserAccountRegister.Text = "";
            pbxUserPasswordRegister.Password = "";
            pbxSurePasswordRegister.Password = "";
            tbxUserAnswerRegister.Text = "";
            tbxUserPasswordRegister.Text = "";
            tbxSurePasswordRegister.Text = "";

        }
      
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
        private async void ChangePsw_Click(object sender, RoutedEventArgs e)//修改密码事件
        {
            ViewModelChangePsw viewModelChangePsw = new ViewModelChangePsw();
            viewModelChangePsw.Account = tbxUserAccountChangePwd.Text;
            viewModelChangePsw.OldPassword = pbxOldPasswordChangePwd.Password;
            viewModelChangePsw.NewPassword = pbxUserPasswordChangePwd.Password;
            viewModelChangePsw.SurePassword = pbxSurePasswordChangePwd.Password;
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            viewModelInformation = await ChangePswView(viewModelChangePsw);

        }

        private void changePwdBack_click(object sender, RoutedEventArgs e)//修改密码界面的返回事件
        {
            ChangePasswordWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// 修改密码信息提交webapi
        /// </summary>
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

        #region 拾回密码和重置密码事件

        Dictionary<string, string> dic2 = new Dictionary<string, string>();
        private async void EtrievePwd_Click(object sender, RoutedEventArgs e)//找回密码事件
        {
            try
            {
                if (tbxUserAnswerEtrievePwd.Text != "")
                {
                    if (pbxUserPasswordResetPwd.Password != "")
                    {
                        #region 密码规范
                        int number = 0, character = 0;
                        foreach (char c in pbxUserPasswordResetPwd.Password)   //规范密码必须由ASCII码33~126之间的字符构成
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
                        #endregion
                        if (pbxUserPasswordResetPwd.Password == pbxSurePasswordResetPwd.Password)
                        {
                            ViewModelRetrievePsw viewModelRetrievePsw = new ViewModelRetrievePsw();
                            viewModelRetrievePsw.Account = tbxUserAccountEtrievePwd.Text;
                            viewModelRetrievePsw.QuestionOrAnswer = CreateMD5.EncryptWithMD5(cbxRetrieve.Text + tbxUserAnswerEtrievePwd.Text);
                            viewModelRetrievePsw.NewPassword = CreateMD5.EncryptWithMD5(pbxUserPasswordResetPwd.Password);
                            viewModelRetrievePsw.SurePassword = CreateMD5.EncryptWithMD5(pbxSurePasswordResetPwd.Password);
                            ViewModelInformation viewModelInformation = new ViewModelInformation();
                            viewModelInformation = await EtrievePwdView(viewModelRetrievePsw);
                        }
                        else
                        {
                            throw new Exception("两次输入的密码不一致！");
                        }
                    }
                    else
                    {
                        throw new Exception("新密码不能为空！");
                    }
                }
                else
                {
                    throw new Exception("请您输入密码拾回问题答案！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册失败！错误信息：\n" + ex.Message);
            }
        }

        private void etrievePwdBack_Click(object sender, RoutedEventArgs e)//找回密码界面的返回事件
        {
            RetrievePasswordWindow.Visibility = Visibility.Collapsed;
            LoginWindow.Visibility = Visibility.Visible;
            tbxUserAccountEtrievePwd.Text = "";
            tbxUserAnswerEtrievePwd.Text = "";
        }


        private void CbxRetrieve_Loaded(object sender, RoutedEventArgs e)//找回密码界面的combobox的初始化下拉菜单事件
        {
            dic2.Add("你最喜欢的颜色是", "1");
            dic2.Add("你的生日是", "2");
            dic2.Add("你的父亲叫什么名字", "3");
            dic2.Add("你最喜欢做什么", "4");
            dic2.Add("你的梦想是", "5");
            cbxRetrieve.ItemsSource = dic;
            cbxRetrieve.DisplayMemberPath = "Key";
            cbxRetrieve.SelectedIndex = 0;
        }

        private void CbxRetrieve_SelectionChanged(object sender, SelectionChangedEventArgs e)//清空密保问题的答案事件
        {
            tbxUserAnswerEtrievePwd.Text = "";
        }

        /// <summary>
        /// 找回密码信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> EtrievePwdView(ViewModelRetrievePsw viewModelRetrievePsw)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/RetrievePsw/PostRetrievePsw", viewModelRetrievePsw);
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
        private void UserAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)//登录界面combobox的SelectionChanged事件
        {
            var users = unitOfWork.DataRepository.Get();
            if (cbxUserAccountLogin.SelectedValue != null)//获取combobox选择的值
            {
                var sysUser = users.Where(s => s.UserAccount.Equals(cbxUserAccountLogin.SelectedValue.ToString())).FirstOrDefault();
                if (sysUser != null)
                {
                    //combobox选中的账号是否记忆密码
                    if (sysUser.RememberPassword == "1")
                    {
                        pbxUserPasswordLogin.Password = CreateMD5.EncryptWithMD5(sysUser.UserPassword);//给passwordbox一串固定密码
                        cheRememberPwdLogin.IsChecked = true;
                    }
                    else
                    {
                        pbxUserPasswordLogin.Password = "";
                        cheRememberPwdLogin.IsChecked = false;
                    }
                }
            }

        }

        #region 用户管理界面
        
        private async void UserManage_Click(object sender, RoutedEventArgs e)//用户管理按钮事件
        {
            AdminWindow.Visibility = Visibility.Collapsed;
            ListViewWindow.Visibility = Visibility.Visible;          
            var viewModelUserManage = await UserManage();
            ListView.ItemsSource = viewModelUserManage;//listview绑定数据源
            ListView.SelectedIndex = 0;
            ListView.Items.Refresh();
        }

        private void BtnChangeRoleListView_Click(object sender, RoutedEventArgs e)//用户管理界面的“修改角色”按钮事件
        {
            ChangeRoleWindow.Visibility = Visibility.Visible;
            ListViewWindow.Visibility = Visibility.Collapsed;
            Height = 354.131;
            Width = 350.841;
            var viewModel = (ViewModelUserManage)ListView.SelectedItem;
            LabTextChangeRole.Content = "更改" + viewModel.ViewUserAccount + "用户的角色";
        }

        private void BtnBackListView_Click(object sender, RoutedEventArgs e)//用户管理界面的返回按钮事件
        {
            AdminWindow.Visibility = Visibility.Visible;
            ListViewWindow.Visibility = Visibility.Collapsed;
        }

        private async void BtnDeleteListView_Click(object sender, RoutedEventArgs e)//删除按钮事件
        {
            var viewModel = (ViewModelUserManage)ListView.SelectedItem;//获取listview选中的那一行
            ViewModelChangeRole viewModelChangeRole = new ViewModelChangeRole();
            viewModelChangeRole.Account = viewModel.ViewUserAccount;
            viewModelChangeRole.UserRole = viewModel.ViewRoleName;
            ViewModelInformation viewModelInformation = new ViewModelInformation();
            viewModelInformation = await PostDeleteUser(viewModelChangeRole);
            //重新从远端数据库获取信息并重新绑定listview
            var viewModelUserManage = await UserManage();
            ListView.ItemsSource = viewModelUserManage;//listview绑定数据源
            ListView.SelectedIndex = 0;
            ListView.Items.Refresh();
            //combobox的刷新
            var user = unitOfWork.DataRepository.Get().Where(s => s.UserAccount.Equals(viewModel.ViewUserAccount)).FirstOrDefault();
            if (user != null)
            {
                unitOfWork.DataRepository.Delete(user);//删除数据库中SysUser表相应的值
                unitOfWork.Save();//保存数据库              
            }
            var users = unitOfWork.DataRepository.Get();
            cbxUserAccountLogin.ItemsSource = users.ToList();       //combobox数据源连接数据库
            cbxUserAccountLogin.DisplayMemberPath = "UserAccount";  //combobox下拉显示的值
            cbxUserAccountLogin.SelectedValuePath = "UserAccount";  //combobox选中项显示的值
            cbxUserAccountLogin.SelectedIndex = 0;               //登陆界面 combobox初始显示第一项
        }

        /// <summary>
        /// 删除用户信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> PostDeleteUser(ViewModelChangeRole viewModelChangeRole)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/DeleteUser/PostDeleteUser", viewModelChangeRole);
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
        /// 从webapi获取用户管理信息
        /// </summary>
        public async Task<List<ViewModelUserManage>> UserManage()
        {
            var response = await client.GetAsync("http://localhost:60033/api/UserManage/GetUserManageInformation");
            response.EnsureSuccessStatusCode();
            var viewModelUserManage = await response.Content.ReadAsAsync<List<ViewModelUserManage>>();
            return viewModelUserManage;
        }
        #endregion

        #region 修改角色界面
        private async void ManagementWindowChangeRole_Click(object sender, RoutedEventArgs e)//修改用户角色界面的“修改”按钮事件
        {
            try
            {
                if (cheStudentChangeRole.IsChecked == false && cheTeacherChangeRole.IsChecked == false && cheAdminChangeRole.IsChecked == false)
                {
                    throw new Exception("请选择角色！");
                }
                else if (cheTeacherChangeRole.IsChecked == false && cheAdminChangeRole.IsChecked == true && cheStudentChangeRole.IsChecked == false || cheTeacherChangeRole.IsChecked == true && cheAdminChangeRole.IsChecked == false && cheStudentChangeRole.IsChecked == false || cheTeacherChangeRole.IsChecked == false && cheAdminChangeRole.IsChecked == false && cheStudentChangeRole.IsChecked == true)
                {
                    var viewModel = (ViewModelUserManage)ListView.SelectedItem;//获取listview选中的那一行数据
                    ViewModelChangeRole viewModelChangeRole = new ViewModelChangeRole();
                    viewModelChangeRole.Account = viewModel.ViewUserAccount;
                    viewModelChangeRole.UserRole = viewModel.ViewRoleName;
                    if(cheTeacherChangeRole.IsChecked == true)
                    {
                        viewModelChangeRole.ChangeRoleInformation = "教师";
                    }
                    else if (cheStudentChangeRole.IsChecked == true)
                    {
                        viewModelChangeRole.ChangeRoleInformation = "学生";
                    }
                    else
                    {
                        viewModelChangeRole.ChangeRoleInformation = "admin";
                    }
                    //修改角色信息提交webapi
                    ViewModelInformation viewModelInformation = new ViewModelInformation();
                    viewModelInformation = await PostChangeRole(viewModelChangeRole);
                    //重新从远端数据库获取信息并重新绑定listview
                    var viewModelUserManage = await UserManage();
                    ListView.ItemsSource = viewModelUserManage;//listview绑定数据源
                    ListView.SelectedIndex = 0;
                    ListView.Items.Refresh();                  
                    //自动返回ListView界面
                    ChangeRoleWindow.Visibility = Visibility.Collapsed;
                    ListViewWindow.Visibility = Visibility.Visible;
                    Height = 421;
                    Width = 656;

                    MessageBox.Show(viewModelInformation.Message);

                    //combobox的刷新
                    var user = unitOfWork.DataRepository.Get();         //combobox的更新
                    cbxUserAccountLogin.ItemsSource = user.ToList();       //combobox数据源连接数据库
                    cbxUserAccountLogin.DisplayMemberPath = "UserAccount";  //combobox下拉显示的值
                    cbxUserAccountLogin.SelectedValuePath = "UserAccount";  //combobox选中项显示的值
                    cbxUserAccountLogin.SelectedIndex = 0;               //登陆界面 combobox初始显示第一项   

                }
                else
                {
                    throw new Exception("只能选择一位角色，请重新选择！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改失败！错误信息：\n" + ex.Message);
            }
        }

        private void ChangeRoleWindowReturn_Click(object sender, RoutedEventArgs e)//修改角色界面的返回按钮事件
        {
            ChangeRoleWindow.Visibility = Visibility.Collapsed;
            ListViewWindow.Visibility = Visibility.Visible;
            Height = 421;
            Width = 656;
        }


        /// <summary>
        /// 修改角色信息提交webapi
        /// </summary>
        private async Task<ViewModelInformation> PostChangeRole(ViewModelChangeRole viewModelChangeRole)
        {
            //异常中断，程序不会破溃
            ViewModelInformation viewModelInformation = null;
            try
            {
                //Post异步提交信息，格式为Json
                var response = await client.PostAsJsonAsync("http://localhost:60033/api/ChangeUserRole/PostChangeUserRole", viewModelChangeRole);
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


    }
}