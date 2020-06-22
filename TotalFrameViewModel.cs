using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace KIA_Flxble_Client.ViewModel
{
    using Model;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Data;
    using System.Windows;
    using ViewModel;
    using Common;
    using KIA_Flxble_Client.View;
    using System.Windows.Media;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using Network;
    using System.Threading;
    using System.Windows.Threading;

    public class TotalFrameViewModel : ViewModelBase
    {
        private RelayCommand _mouseDownCommand;
        private RelayCommand _mouseUpCommand;
        private RelayCommand _mouseMoveCommand;

        private RelayCommand _mouseLeftButtonDownCommand;
        private RelayCommand _previewMouseDownCommand;
        private RelayCommand _previewMouseUpCommand;

        private RelayCommand _touchUpCommand;
        private RelayCommand _touchDownCommand;
        private RelayCommand _touchMoveCommand;

        private TotalFrameView tfvUC = null;
        private double md = 0;
        private double mu = 0;
        private DispatcherTimer _timer;
        private System.Diagnostics.Stopwatch _stopwatch;

        private bool _isDispose = false; // 소멸자에 사용
        private bool _isShowing = true;



        #region RelayCommand

        public RelayCommand FrameButtonCommand { get; set; }

        public RelayCommand MouseMoveCommand
        {
            get
            {
                if (_mouseMoveCommand == null) return _mouseMoveCommand = new RelayCommand(param => ExecuteMouseMove((MouseEventArgs)param));
                return _mouseMoveCommand;
            }
            set { _mouseMoveCommand = value; }
        }
        public RelayCommand MouseUpCommand
        {
            get
            {
                if (_mouseUpCommand == null) return _mouseUpCommand = new RelayCommand(param => ExecuteMouseUp((MouseEventArgs)param));
                return _mouseUpCommand;
            }
            set { _mouseUpCommand = value; }
        }

        public RelayCommand MouseDownCommand
        {
            get
            {
                if (_mouseDownCommand == null) return _mouseDownCommand = new RelayCommand(param => ExecuteMouseDown((MouseEventArgs)param));
                return _mouseDownCommand;
            }
            set { _mouseDownCommand = value; }
        }


        public RelayCommand PreviewMouseDownCommand
        {
            get
            {
                if (_previewMouseDownCommand == null) return _previewMouseDownCommand = new RelayCommand(param => ExecutePreviewMouseDown((MouseEventArgs)param));
                return _previewMouseDownCommand;
            }
            set { _previewMouseDownCommand = value; }
        }

        public RelayCommand PreviewMouseUpCommand
        {
            get
            {
                if (_previewMouseUpCommand == null) return _previewMouseUpCommand = new RelayCommand(param => ExecutePreviewMouseUp((MouseEventArgs)param));
                return _previewMouseUpCommand;
            }
            set { _previewMouseUpCommand = value; }
        }


        public RelayCommand MouseLeftButtonDownCommand
        {
            get
            {
                if (_mouseLeftButtonDownCommand == null) return _mouseLeftButtonDownCommand = new RelayCommand(param => ExecuteLeftButtonMouseDown((MouseEventArgs)param));
                return _mouseLeftButtonDownCommand;
            }
            set { _mouseLeftButtonDownCommand = value; }
        }

        public RelayCommand TouchUpCommand
        {
            get
            {
                if (_touchUpCommand == null) return _touchUpCommand = new RelayCommand(param => ExecuteTouchUp((TouchEventArgs)param));
                return _touchUpCommand;
            }
            set { _touchUpCommand = value; }
        }

        public RelayCommand TouchDownCommand
        {
            get
            {
                if (_touchDownCommand == null) return _touchDownCommand = new RelayCommand(param => ExecuteTouchDown((TouchEventArgs)param));
                return _touchDownCommand;
            }
            set { _touchDownCommand = value; }
        }

        public RelayCommand TouchMoveCommand
        {
            get
            {
                if (_touchMoveCommand == null) return _touchMoveCommand = new RelayCommand(param => ExecuteTouchMove((TouchEventArgs)param));
                return _touchMoveCommand;
            }
            set { _touchMoveCommand = value; }
        }

        #endregion

        #region Constructor
        public TotalFrameViewModel()
        {
            /*** 데이터가 있다고 가정(디시리얼라이즈) ***/
            Entity.TotalDatas = EFPManager.Instance.Deserialize();
            /// 저장할때 동일한 이름이 없도록 한다


            /// 버튼 클릭 이벤트
            FrameButtonCommand = new RelayCommand(Execute, CanExecute);


            EFPManager.Instance.MouseEventType = MouseEventType.Non;
            Entity.PageType = PageType.Page1;

            _netAndCom_StateColor = Brushes.Red;


            _stopwatch = new System.Diagnostics.Stopwatch();
            /// 5초 후 3초 주기로 갱신
            StartTimer(null, 5000, 3000);
        }
        #endregion

        ~TotalFrameViewModel()
        {
            if (!_isDispose)
                Dispose();
        }
        public void Dispose()
        {
            StopTimer();
        }

        #region Timer & Network
        private void StartTimer(object state, int dueTime, int period)
        {
            //_timer = new Timer(ThreadingTimerCallBack, state, dueTime, period);
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += ThreadingTimerCallBack;
            _timer.Start();
        }
        private void StopTimer()
        {
            //_timer.Change(Timeout.Infinite, Timeout.Infinite);
            //_stopwatch.Stop();
            //_stopwatch.Reset();
            _timer.Stop();
        }

        int nNetCount = 0;
        private void ThreadingTimerCallBack(object sender, EventArgs e)
        {
            //MainWindowViewModel.LogViewModel.SetLogMessage(state.ToString());
            UDPClientModule.Instance.SendNetworkState();

            if (UDPClientModule.Instance.IsConnectingNetwork)
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() != typeof(MainWindow))
                        continue;

                    foreach (UserControl uc in FindVisualChildren<UserControl>(window))
                    {
                        if (uc.Name == "TotalFrameView")
                        {
                            ((TotalFrameView)uc).NetRectangle.Fill = Brushes.Green;
                            ((TotalFrameView)uc).ComRectangle.Fill = Brushes.Green;
                        }
                    }
                }
                //Color col = Colors.Green;
                //NetAndCom_StateColor = new SolidColorBrush(col);
                //nNetCount = 0;

            }
            else if (nNetCount > 3)
            {

                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() != typeof(MainWindow))
                        continue;

                    foreach (UserControl uc in FindVisualChildren<UserControl>(window))
                    {
                        if (uc.Name == "TotalFrameView")
                        {
                            ((TotalFrameView)uc).NetRectangle.Fill = Brushes.Red;
                            ((TotalFrameView)uc).ComRectangle.Fill = Brushes.Red;
                        }
                    }
                }

                //TotalFrameView tfv = (TotalFrameView)FindUserControl("TotalFrameUC");
                //tfv.NetRectangle.Fill = Brushes.Red;
                //Color col = Colors.Red;
                //NetAndCom_StateColor = new SolidColorBrush(col);
            }
            nNetCount++;
            //if()
            //    nNetCount++;


        }


        #endregion

        #region Event
        public bool CanExecute(object parameter)
        {
            return true;
        }


        /// <summary>
        /// Button Event
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            ButtonMode(parameter.ToString());
        }

        private void ButtonMode(string mode)
        {
            switch (mode)
            {
                case "DeleteFrameButton":
                    DeleteFrameButton();
                    EFPManager.Instance.ButtonState = ButtonState.Non;
                    break;

                case "AddFrameButton":
                    AddFrameButton();
                    break;

                case "EmergencyStopButton":
                    EmergencyStopButton();
                    break;

                case "ResetButton":
                    ResetButton();
                    break;

                case "CloseFrameButton":
                    foreach(Window window in Application.Current.Windows)
                    {
                        if(window.GetType() == typeof(MainWindow))
                        {
                            window.Close();
                        }
                    }
                    EFPManager.Instance.ButtonState = ButtonState.ButtonStop;
                    break;

                case "TFV_SaveButton":
                    TFV_SaveButton();
                    break;
            }
        }


        private SolidColorBrush _netAndCom_StateColor;
        public SolidColorBrush NetAndCom_StateColor
        {
            get { return _netAndCom_StateColor; }
            set
            {
                _netAndCom_StateColor = value;
            }
        }



        #region DeleteFrameButton
        private void DeleteFrameButton()
        {
            /// checkbox Visibility
            TotalFrameView tfv = (TotalFrameView)FindUserControl("TotalFrameView");

            if (_isShowing)
            {
                foreach (UserControl uc in FindVisualChildren<UserControl>(tfv))
                {
                    if (uc.Name == "FPView")
                    {
                        foreach (Button cb in FindVisualChildren<Button>(((FramePropertyView)uc)))
                        {
                            if(cb.Name == "FPV_DeletCheckButton")
                                cb.Visibility = Visibility.Visible;
                        }
                    }
                }
                /// Save Button Visibility
                tfv.SaveButton.Visibility = Visibility.Visible;

                _isShowing = false;
                EFPManager.Instance.IsTouch = true;     // 터치 상태 확인
                EFPManager.Instance.ButtonState = ButtonState.ButtonRunning;    // 버튼 실행 중

            }
            /// Hidden
            else
            {
                foreach (UserControl uc in FindVisualChildren<UserControl>(tfv))
                {
                    if (uc.Name == "FPView")
                    {
                        foreach (Button cb in FindVisualChildren<Button>(((FramePropertyView)uc)))
                        {
                            if(cb.Name == "FPV_DeletCheckButton")
                            {

                                foreach(var item in Entity.TotalDatas)
                                {
                                    if (item.IsCheckBox != true)
                                        continue;

                                    cb.Background = Brushes.White;
                                }

                                EFPManager.Instance.IsCheckDeletFrame = false;
                                cb.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                }
                /// Save Button Visibility
                tfv.SaveButton.Visibility = Visibility.Hidden;

                _isShowing = true;
                EFPManager.Instance.IsTouch = false;
                EFPManager.Instance.ButtonState = ButtonState.ButtonStop;    // 버튼 실행 정지
            }
           
        }
        #endregion

        #region AddFrameButton

        private void AddFrameButton()
        {
            if (EFPManager.Instance.AddEditorFrameWindow != null)
                return;

            EFPManager.Instance.ButtonState = ButtonState.ButtonRunning;

            EFPManager.Instance.AddEditorFrameWindow = new AddEditorFramePropertyWindowView
            {
                //Background = Brushes.Black,
                Width = 920,
                Height = 630,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true
            };
            EFPManager.Instance.ChangeParentWindow_Gray();
            EFPManager.Instance.AddEditorFrameWindow.Show();

            //EFPManager.Instance.AddEditorFrameWindow.Add(aefpWindowView);
        }
        #endregion

        #region EmergencyStopButton
        private void EmergencyStopButton()
        {
            MessageBox_YesNo("Are you sure you want to Emergency Stop it?", "Emergency Stop Button", "stop");
        }
        #endregion

        #region ResetButton
        private void ResetButton()
        {
            MessageBox_YesNo("Are you sure you want to Reset it?", "Reset Button", "reset");
        }


        private void MessageBox_YesNo(string message, string caption, string opMessage)
        {
            if(MessageBox.Show(message, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                UDPClientModule.Instance.SendButtonMessage(opMessage);
            }
        }
        #endregion

        #region TFV_SaveButton
        private void TFV_SaveButton()
        {
            List<int> removeID = new List<int>();
            foreach(var item in Entity.TotalDatas)
            {
                if(item.IsCheckBox)
                {
                    //Entity.TotalDatas.RemoveAt(item.ID);
                    removeID.Add(item.ID);
                }
            }

            /// 삭제
            foreach(var item in removeID)
            {
                Entity.TotalDatas.RemoveAt(item);
            }

            // 정렬
            for(int i = 0; i < Entity.TotalDatas.Count; i++)
            {
                Entity.TotalDatas[i].ID = i;
                Entity.TotalDatas[i] = Entity.TotalDatas[i];
            }

            // 모자란 부분 추가
            //int addNewFrameCount = 32 - Entity.TotalDatas.Count;

            for(int i = Entity.TotalDatas.Count; i < 32; i++)
            {
                Entity.TotalDatas.Add(new TotalData
                    (
                        "T I T L E",
                        "/Resources/EmptyFrame.png",
                        new FrameModel(PropertyState.AUTO, "LENGTH FRONT", "", "length_front"),
                        new FrameModel(PropertyState.AUTO, "LENGTH REAR", "", "length_rear"),
                        new FrameModel(PropertyState.AUTO, "HEIGHT FRONT FIRST", "", "height_front_first"),
                        new FrameModel(PropertyState.AUTO, "HEIGHT FRONT SECOND", "", "height_front_second"),
                        new FrameModel(PropertyState.AUTO, "HEIGHT REAR", "", "height_rear"),
                        new FrameModel(PropertyState.HAND, "WIDTH", "", "width"),
                        new FrameModel(PropertyState.HAND, "HANDLE", "", "handle"),
                        new FrameModel(PropertyState.HAND, "PEDAL", "", "pedal"),
                        new FrameModel(PropertyState.HAND, "SEAT X", "", "seat_x"),
                        new FrameModel(PropertyState.HAND, "SEAT Y", "", "seat_y"),

                        new FrameModel(PropertyState.TOTAL, "WIDTH", "", null),     // Width와 동일
                        new FrameModel(PropertyState.TOTAL, "LENGTH", "", null),    // Lenght front + lenght rear
                        new FrameModel(PropertyState.TOTAL, "HEIGHT", "1515", null),     // height rear
                        i
                    ));
            }

            /// 페이지 체인지
            /// 

            ObservableCollection<FramePropertyViewModel> fpVM = new ObservableCollection<FramePropertyViewModel>();

            switch (Entity.PageType)
            {
                case PageType.Page1:

                    for (int i = 0; i < 8; i++)
                        fpVM.Add(new FramePropertyViewModel(null, new FramePropertyModel(Entity.TotalDatas[i].FrameName, Entity.TotalDatas[i].FrameImage, Entity.TotalDatas[i].ID, EFPManager.Instance.GetFrameData(i))));
                    break;

                case PageType.Page2:
                    for (int i = 7; i < 16; i++)
                        fpVM.Add(new FramePropertyViewModel(null, new FramePropertyModel(Entity.TotalDatas[i].FrameName, Entity.TotalDatas[i].FrameImage, Entity.TotalDatas[i].ID, EFPManager.Instance.GetFrameData(i))));
                    break;

                case PageType.Page3:
                    for (int i = 15; i < 24; i++)
                        fpVM.Add(new FramePropertyViewModel(null, new FramePropertyModel(Entity.TotalDatas[i].FrameName, Entity.TotalDatas[i].FrameImage, Entity.TotalDatas[i].ID, EFPManager.Instance.GetFrameData(i))));
                    break;

                case PageType.Page4:
                    for (int i = 23; i < 32; i++)
                        fpVM.Add(new FramePropertyViewModel(null, new FramePropertyModel(Entity.TotalDatas[i].FrameName, Entity.TotalDatas[i].FrameImage, Entity.TotalDatas[i].ID, EFPManager.Instance.GetFrameData(i))));
                    break;
            }

            LFPView lfpv = (LFPView)FindUserControl("ListFPView");

            lfpv.TFV_listView.ItemsSource = fpVM;
            tfvUC.contentPresenter.Content = lfpv;

            tfvUC.SaveButton.Visibility = Visibility.Hidden;

            _isShowing = true;
            EFPManager.Instance.IsTouch = false;
            EFPManager.Instance.Serialize();
        }
        #endregion



        private void ExecuteMouseMove(MouseEventArgs e)
        {

        }

        private void ExecuteMouseUp(MouseEventArgs e)
        {
        }

        private void ExecuteMouseDown(MouseEventArgs e)
        {
        }

        private void ExecuteLeftButtonMouseDown(MouseEventArgs e)
        {
        }


        bool isTouchCheck = false;
        #region Touch 
        private void ExecuteTouchDown(TouchEventArgs e)
        {
            //EFPManager.Instance.IsTouch = true;
            EFPManager.Instance.MouseEventType = MouseEventType.TouchDown;
            Point pos;
            var uc = FindUserControl("TotalFrameView");
            pos = ((TouchEventArgs)e).GetTouchPoint(uc).Position;
            md = pos.X;
            isTouchCheck = true;
        }

        private void ExecuteTouchUp(TouchEventArgs e)
        {
            //EFPManager.Instance.IsTouch = true;

            var uc = FindUserControl("TotalFrameView");
            Point pos;
            pos = ((TouchEventArgs)e).GetTouchPoint(uc).Position;
            mu = pos.X;
            isTouchCheck = true;

            tfvUC = (TotalFrameView)uc;
            TransitionType = PageTransitionType.Slide;



            /// 추가 & 삭제 모드에서는 애니메이션 동작 안함
            /// 버튼이 클릭 되면 true
            //if (_isButtonState)
            //    return;

            if (EFPManager.Instance.IsTouch)
                return;

            StartAnimation(md, mu);
            EFPManager.Instance.MouseEventType = MouseEventType.TouchUp;


        }


        private void ExecuteTouchMove(TouchEventArgs e)
        {
           
        }

        #endregion


        #region Mouse
        private void ExecutePreviewMouseDown(EventArgs e)
        {
            if (isTouchCheck)
            {
                //isTouchCheck = false;
                return;
            }

            EFPManager.Instance.IsTouch = false;
            Point pos;
            var uc = FindUserControl("TotalFrameView");
            pos = uc.PointToScreen(Mouse.GetPosition(uc));
            md = pos.X;

            //Console.WriteLine("Mouse Down : " + e.GetPosition((IInputElement)e.Source));
            EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseDown;
        }




        private void ExecutePreviewMouseUp(MouseEventArgs e)
        {
            if (isTouchCheck)
            {
                isTouchCheck = false;
                return;
            }

            var uc = FindUserControl("TotalFrameView");
            Point pos;
            pos = uc.PointToScreen(Mouse.GetPosition(uc));
            mu = pos.X;


            //Console.WriteLine("Mouse Down : " + e.GetPosition((IInputElement)e.Source));

            tfvUC = (TotalFrameView)uc;
            TransitionType = PageTransitionType.Slide;


            if (EFPManager.Instance.IsTouch)
                return;


            StartAnimation(md,mu);

            EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseUp;
        }

        #endregion


        private void StartAnimation(double down, double up)
        {
            // Right to Left
            if (down - up > 50)
            {
                /// isNextFramePage가 false 이면 애니매이션 동작 안함
                if (!isNextFramePage("RightToLeft"))
                    return;

                LFPView newPage = new LFPView();
                ShowPage(newPage, "RightToLeft");
                PageCheckEllipse();
                EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseUp_MouseMove;
                return;
            }
            /// Left to Right
            else if (down - up < -50)
            {
                /// isNextFramePage가 false 이면 애니매이션 동작 안함
                if (!isNextFramePage("LeftToRight"))
                    return;


                LFPView newPage = new LFPView();
                ShowPage(newPage, "LeftToRight");

                PageCheckEllipse();

                EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseUp_MouseMove;
                return;
            }
            //else
            //{
            //    EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseUp;
            //    return;
            //}
        }




        private bool isNextFramePage(string state)
        {
            if (EFPManager.Instance.EditorFrameWindow != null || EFPManager.Instance.AddEditorFrameWindow != null)
                return false;

            if(state == "RightToLeft")
            {
                switch (Entity.PageType)
                {
                    case PageType.Page1:
                        Entity.PageType = PageType.Page2;
                        return true;

                    case PageType.Page2:
                        Entity.PageType = PageType.Page3;
                        return true;

                    case PageType.Page3:
                        Entity.PageType = PageType.Page4;
                        return true;

                    case PageType.Page4:
                        EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseUp_MouseMove;
                        return false;
                }

            }
            else
            {
                switch (Entity.PageType)
                {
                    case PageType.Page1:
                        EFPManager.Instance.MouseEventType = MouseEventType.PreviewMouseUp_MouseMove;
                        return false;

                    case PageType.Page2:
                        Entity.PageType = PageType.Page1;
                        return true;

                    case PageType.Page3:
                        Entity.PageType = PageType.Page2;
                        return true;

                    case PageType.Page4:
                        Entity.PageType = PageType.Page3;
                        return true;
                }
            }


            return false;
        }


        private void PageCheckEllipse()
        {
            TotalFrameView tfView = (TotalFrameView)FindUserControl("TotalFrameView");

            switch (Entity.PageType)
            {
                case PageType.Page1:
                    tfView.PCE0.Fill = Brushes.White;
                    tfView.PCE1.Fill = Brushes.Gray;
                    tfView.PCE2.Fill = Brushes.Gray;
                    tfView.PCE3.Fill = Brushes.Gray;
                    break;

                case PageType.Page2:
                    tfView.PCE0.Fill = Brushes.Gray;
                    tfView.PCE1.Fill = Brushes.White;
                    tfView.PCE2.Fill = Brushes.Gray;
                    tfView.PCE3.Fill = Brushes.Gray;
                    break;

                case PageType.Page3:
                    tfView.PCE0.Fill = Brushes.Gray;
                    tfView.PCE1.Fill = Brushes.Gray;
                    tfView.PCE2.Fill = Brushes.White;
                    tfView.PCE3.Fill = Brushes.Gray;
                    break;

                case PageType.Page4:
                    tfView.PCE0.Fill = Brushes.Gray;
                    tfView.PCE1.Fill = Brushes.Gray;
                    tfView.PCE2.Fill = Brushes.Gray;
                    tfView.PCE3.Fill = Brushes.White;
                    break;
            }

        }





        #endregion

        #region Animation
        Stack<UserControl> pages = new Stack<UserControl>();
        public UserControl CurrentPage { get; set; }


        public void ShowPage(UserControl newPage, string state)
        {
            pages.Push(newPage);

            Task.Factory.StartNew(() => ShowNewPage(state));
        }

        void ShowNewPage(string state)
        {
            //if (tfvUC == null)
            //    return;

            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (state == "RightToLeft")
                {
                    if (tfvUC.contentPresenter.Content != null)
                    {
                        UserControl oldPage = tfvUC.contentPresenter.Content as UserControl;

                        if (oldPage != null)
                        {
                            oldPage.Loaded -= newPage_Loaded;

                            UnloadPage(oldPage);
                        }
                    }
                    else
                    {
                        ShowNextPage();
                    }
                }
                // LeftToRight
                else if(state == "LeftToRight")
                {
                    if (tfvUC.contentPresenter.Content != null)
                    {
                        UserControl oldPage = tfvUC.contentPresenter.Content as UserControl;

                        if (oldPage != null)
                        {
                            oldPage.Loaded -= newPage_Loaded_LeftToRight;
                            UnloadPage_LeftToRight(oldPage);
                        }
                    }
                    else
                    {
                        ShowNextPage_LeftToRight();
                    }
                }
            });
        }

        void ShowNextPage()
        {
            UserControl newPage = pages.Pop();

            newPage.Loaded += newPage_Loaded;

            tfvUC.contentPresenter.Content = newPage;
        }

        void ShowNextPage_LeftToRight()
        {
            UserControl newPage = pages.Pop();

            newPage.Loaded += newPage_Loaded_LeftToRight;

            tfvUC.contentPresenter.Content = newPage;
        }


        // 오른쪽 -> 왼쪽
        void UnloadPage(UserControl page)
        {
            Storyboard hidePage = (tfvUC.Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();

            hidePage.Completed += hidePage_Completed;

            hidePage.Begin(tfvUC.contentPresenter);
        }

        void newPage_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard showNewPage = tfvUC.Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard;

            showNewPage.Begin(tfvUC.contentPresenter);

            CurrentPage = sender as UserControl;
        }

        // 왼쪽 -> 오른쪽
        void UnloadPage_LeftToRight(UserControl page)
        {
            Storyboard hidePage = (tfvUC.Resources[string.Format("{0}Out_LeftToRight", TransitionType.ToString())] as Storyboard).Clone();

            hidePage.Completed += hidePage_Completed_LeftToRight;

            hidePage.Begin(tfvUC.contentPresenter);
        }

        void newPage_Loaded_LeftToRight(object sender, RoutedEventArgs e)
        {
            Storyboard showNewPage = tfvUC.Resources[string.Format("{0}In_LeftToRight", TransitionType.ToString())] as Storyboard;

            showNewPage.Begin(tfvUC.contentPresenter);

            CurrentPage = sender as UserControl;
        }





        void hidePage_Completed(object sender, EventArgs e)
        {
            tfvUC.contentPresenter.Content = null;

            ShowNextPage();
        }


        void hidePage_Completed_LeftToRight(object sender, EventArgs e)
        {
            tfvUC.contentPresenter.Content = null;

            ShowNextPage_LeftToRight();
        }

        public static readonly DependencyProperty TransitionTypeProperty = DependencyProperty.Register("TransitionType",
        typeof(PageTransitionType),
        typeof(TotalFrameView), new PropertyMetadata(PageTransitionType.SlideAndFade));

        public PageTransitionType TransitionType
        {
            get
            {
                return (PageTransitionType)tfvUC.GetValue(TransitionTypeProperty);
            }
            set
            {
                tfvUC.SetValue(TransitionTypeProperty, value);
            }
        }

        #endregion
    }
}
