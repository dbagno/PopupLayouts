using System;

using Xamarin.Forms;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PopupLayoutsForms
{
    public class PopupLayouts : AbsoluteLayout
    {
        public enum PopupType
        {
            Center,
            Relative,
            Margin,

        }

        public enum TrianglePoistion
        {
            Above,
            Below,
        }

        public delegate void ClosedEventHandler(bool changed);

        public delegate void OpenedEventHandler();

        public event ClosedEventHandler Closed;

        public event OpenedEventHandler Opened;

        protected virtual void OnClosed(bool changed)
        {
            Closed?.Invoke(PopupChanged);
        }

        protected virtual void OnOpened()
        {
            Opened?.Invoke();
        }

        private PopupType _popupType;
        private TrianglePoistion _trianglePosition;

        private View _pageView{ get; set; }

        public View PopupParent { get; set; }

        private ContentPage _parentPage{ get; set; }

        private double _lastwidth = 0;
        private double _lastheight = 0;
        private bool _isModal = false;
        private double _scale = 0;

        private StackLayout _popupShield { get; set; }

        private View _popupContent { get; set; }

        private View _contentArea{ get; set; }

        public bool PopupChanged = false;
        private double _popupWidth = 0;
        private double _popupHeight = 0;
        private double _popupLeft = 0;
        private double _popupTop = 0;
        private double _left = 0;
        private double _top = 0;
        private double _right = 0;
        private double _bottom = 0;
        private double _width = 0;
        private double _height = 0;
        private string _title = "";

         
        private double _scrollVset = 0;

        private ScrollView _parentScroll{ get; set; }

        private double _scrollOffset = 0;

        private Rectangle _marginRect{ get; set; }

        private View _relativeTo{ get; set; }

        private StackLayout _popupStack{ get; set; }

        private Frame _popupFrame{ get; set; }

        private View _popupFramer { get; set; }

        private Image _triangleImage{ get; set; }

        private StackLayout _triangleStack{ get; set; }

        private double _oheight = 0;
        private double _owidth = 0;
        public bool PopupVisible = false;
        public double StatusBarOffset = 1;
        private double LastScrollY = 0;

        public async void DismisPopup()
        {
            await _popupFramer.ScaleTo(0, 120, Easing.Linear);
            Children.Remove(_popupFramer);
           
            _popupFramer = null;
            _popupFrame = null;
            _popupStack = null;
            if (_isModal)
            {
                Children.Remove(_popupShield);
                _popupShield = null;
            }
            PopupVisible = false;
            OnClosed(PopupChanged);
        }

        public double GetX(View view)
        {
            var x = view.X;

            var parentX = view.ParentView;

            try
            {
                while (parentX != null)
                {
                    x += parentX.X;
                    parentX = parentX.ParentView;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return x;
        }

        public Point GetXY(View view)
        {
            _trianglePosition = TrianglePoistion.Above;
            
            var x = view.X;

            var parentX = view.ParentView;

            try
            {
                while (parentX != null)
                {
                    x += parentX.X;
                    parentX = parentX.ParentView;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (x <= 0)
            {
                x = 1;
            }
            if (x + _width >= _contentArea.Width)
            {
                x = _contentArea.Width - (_width + 1);
                if (x < 0)
                {
                    x = 1;
                    _width = _contentArea.Width - 2;
                }
                
            }
             
            var y = view.Y;
           
            var parentY = view.ParentView;
           
            try
            {
                while (parentY != null)
                {

                    if (parentY.GetType().Name == "ScrollView")
                    {
                        if ((parentY as ScrollView).ScrollY > 0)
                        {
                            var _yScroll = (parentY as ScrollView);
                            _scrollOffset = _yScroll.ScrollY;
                           
                            y -= _scrollOffset;
                        }
                    }
                    else
                    {
                        y += parentY.Y;
                       
                    }
                    parentY = parentY.ParentView;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            y += _scrollVset;
            if (y <= 0)
            {
                y = 1;
            }
            var y1 = y + _relativeTo.Height / 2;
            var y2 = _contentArea.Height - (y + _relativeTo.Height / 2);
            if (y1 > y2)
            {
                if (_height > y1)
                {
                    _height = y1 -= (_relativeTo.Height / 2) + 1;
                }
                _trianglePosition = TrianglePoistion.Below;
              
                y -= _height; 
                if (y < 1)
                    y = 1;
            }
            else
            {
                if (_height > y2)
                {
                    _height = (y2 -= _relativeTo.Height / 2);
                    
                }
                _trianglePosition = TrianglePoistion.Above;
                y += _relativeTo.Height;
                if (y + _height >= _contentArea.Height)
                {
                    var dif = Math.Abs(y + _height - _contentArea.Height);
                    _height -= dif;
                    if (Math.Abs(dif) < .001)
                    {
                        _height -= 1;
                    }
                   
                }
                
             
            }
            return new Point(x, y);
        }

        public PopupLayouts(View view, ContentPage parentPage, ScrollView parentScroll = null)
        {
            _parentPage = parentPage;
            _contentArea = view;
            _lastwidth = parentPage.Width;
            _lastwidth = parentPage.Height;
            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;
            BackgroundColor = Color.Transparent;
            _scrollVset = 0;
            if (parentScroll != null)
            {
                if (_parentScroll == null)
                {
                    this._parentScroll = parentScroll;
                    this._scrollVset = parentScroll.Y;   
                }
                this._parentScroll.Scrolled += ((object sender, ScrolledEventArgs e) =>
                {
                    if (e.ScrollY > 0)
                    {
                        this.LastScrollY = e.ScrollY;
                    }
                });
            }
            Children.Add(view, new Rectangle(0f, 0f, parentPage.Width - parentPage.Padding.HorizontalThickness, parentPage.Height - parentPage.Padding.VerticalThickness), AbsoluteLayoutFlags.None);
             
            _parentPage.SizeChanged += (async(object sender, EventArgs e) =>
            {
                if (view != null && _parentPage.Width > 0)
                {
                    if (Math.Abs(_lastwidth - _parentPage.Width) > .001)
                    {
                        _lastwidth = _parentPage.Width;
                        _lastheight = _parentPage.Height;
                        SetLayoutBounds(view, new Rectangle(0f, 0f, _parentPage.Width - _parentPage.Padding.HorizontalThickness, _parentPage.Height - _parentPage.Padding.VerticalThickness));
                        if (_parentScroll != null && _relativeTo != null && PopupVisible)
                        {
                            await _parentScroll.ScrollToAsync(_relativeTo, ScrollToPosition.MakeVisible, false);
                        }
                        else if (_parentScroll != null && LastScrollY > 0)
                        {
                            
                            if (_lastwidth > _lastheight)
                            {
                                await _parentScroll.ScrollToAsync(0, LastScrollY + 216, false);
                            }
                            else
                            {
                                await _parentScroll.ScrollToAsync(0, LastScrollY - 216, false);
                            }
                        }
                        if (view != null && PopupVisible)
                        {
                            if (_isModal)
                            {
                                SetLayoutBounds(_popupShield, new Rectangle(0, 0, _parentPage.Width, _parentPage.Height));

                                if (!Children.Contains(_popupShield))
                                {
                                    Children.Add(_popupShield);
                                }
                            }
                           
                            switch (_popupType)
                            {
                                case PopupType.Center:
                                    _popupWidth = _parentPage.Width * _scale;
                                    _popupHeight = _parentPage.Height * _scale;
                                    _popupLeft = ((_parentPage.Width - _parentPage.Padding.HorizontalThickness) - _popupWidth) / 2;
                                    _popupTop = ((_parentPage.Height - _parentPage.Padding.VerticalThickness) - _popupHeight) / 2;
                                    if (_popupFramer != null)
                                    {
                                        SetLayoutBounds(_popupFramer, new Rectangle(_popupLeft, _popupTop, _popupWidth, _popupHeight));
                                    }
                                    break;
                                case PopupType.Relative:
                                    _width = _owidth;
                                    _height = _oheight;
                                    Device.BeginInvokeOnMainThread(() =>
                                        {
                                            Children.Remove(_popupFramer);
                                            _popupFramer = null;
                                            _popupFrame = null;
                                            _popupStack = null;
                                            if (_isModal)
                                            {
                                                Children.Remove(_popupShield);
                                                _popupShield = null;
                                            }
                                        });
                                    Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                                        {
                                            Task.Factory.StartNew(delegate
                                                {
                                                    Device.BeginInvokeOnMainThread(() =>
                                                        ShowPopupRelative(_popupContent, _relativeTo, _owidth, _oheight, _isModal, _title));
                                                }); 
                                            return false;
                                        }); 
                                    return;
                                case PopupType.Margin:
                                    _left = _parentPage.Width * _marginRect.Left;
                                    _top = _parentPage.Height * _marginRect.Top;
                                    _right = _parentPage.Width * _marginRect.Right;
                                    _bottom = _parentPage.Height * _marginRect.Bottom;
                                    SetLayoutBounds(_popupFramer, new Rectangle(_left, _top, _parentPage.Width - _right, _parentPage.Height - _bottom));
                                    break;
                                default:
                                    break;
                            }
                            if (_popupFramer != null)
                            {
                                if (!Children.Contains(_popupFramer))
                                {
                                    Children.Add(_popupFramer);
                                }
                            }
                        } 
                    }
                }
            });
        }

        public void DrawPopup(View view, bool rounded, string title)
        {
 
            #region CommonPopupArea
            if (rounded)
            {
                _popupFrame = new Frame
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HasShadow = true,
                    Padding = 4,
                    BackgroundColor = (_popupType == PopupType.Relative) ? BackgroundColor = Color.Transparent : Color.Gray.WithLuminosity(.98),
                    IsClippedToBounds = false,
                };
            }
            else
            {

                _popupStack = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Padding = 0,
                    Spacing = 0,
                    BackgroundColor = (_popupType == PopupType.Relative) ? BackgroundColor = Color.Transparent : Color.Gray.WithLuminosity(.98),
                    IsClippedToBounds = false,
                };
            }
            if (_popupType == PopupType.Relative)
            {
                _triangleStack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Padding = 0,
                    Spacing = 0,
                    BackgroundColor = Color.Transparent,
                    HeightRequest = 16,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                };
                _triangleImage = new Image
                {
                    Source = (_trianglePosition == TrianglePoistion.Below) ? ImageSource.FromFile("down_triangle.png") : ImageSource.FromFile("up_triangle.png"),
                    HeightRequest = 16,
                    WidthRequest = 20,
                    Aspect = Aspect.AspectFit,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = (_trianglePosition == TrianglePoistion.Below) ? LayoutOptions.End : LayoutOptions.Start,
                    BackgroundColor = Color.Transparent,
                };
                _triangleStack.Children.Add(_triangleImage);
            }
         
            var titleBarStack = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                Padding = new Thickness(5, 1, 5, 1),
                Spacing = 1,
                HeightRequest = 26,
                BackgroundColor = (rounded) ? Color.White : Color.Gray.WithLuminosity(.85),
            };
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => DismisPopup();
            titleBarStack.GestureRecognizers.Add(tapGestureRecognizer);
            var seperator = new BoxView{ HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.Start, HeightRequest = .5, BackgroundColor = Color.Gray.WithLuminosity(.8) };
           
            var closeLabel = new Label
            {
                Text = "X",
                HorizontalTextAlignment = TextAlignment.End,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 18,
                WidthRequest = 26,
                HeightRequest = 26,
                TextColor = Color.Gray.WithLuminosity(.3),
                HorizontalOptions = LayoutOptions.End,
               
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.Transparent,

            };
            var titleLabel = new Label
            {
                Text = (title == "") ? "   " : title,
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                FontSize = 18,
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,

                LineBreakMode = LineBreakMode.TailTruncation,
                TextColor = Color.Gray.WithLuminosity(.3), 
            };
            titleBarStack.Children.Add(titleLabel);
            var buttonStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.EndAndExpand,
            };
            //buttonStack.Children.Add(closeButton);
            buttonStack.Children.Add(closeLabel);

            titleBarStack.Children.Add(buttonStack);
            if (rounded)
            {

                var frameContent = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    Padding = 0,
                    Spacing = 0,
                };
                frameContent.Children.Add(titleBarStack);
                frameContent.Children.Add(seperator);
                frameContent.Children.Add(view);
                _popupFrame.Content = frameContent;
            }
            else
            {
          
                if (_popupType == PopupType.Relative && _trianglePosition == TrianglePoistion.Above)
                {
                    _popupStack.Children.Add(_triangleStack);
                    //add triangle to top
                }
               
                _popupStack.Children.Add(titleBarStack);
                //_popupStack.Children.Add(seperator);
                if (_popupType == PopupType.Relative)
                {
                    var viewWrapper = new StackLayout
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        Padding = 1,
                        BackgroundColor = Color.Gray.WithLuminosity(.85),
                    };
                    viewWrapper.Children.Add(view);
                    _popupStack.Children.Add(viewWrapper);
                }
                else
                {
                    _popupStack.Children.Add(view);
                }
               

                if (_popupType == PopupType.Relative && _trianglePosition == TrianglePoistion.Below)
                {
                  
                    _popupStack.Children.Add(_triangleStack);
                    //add triangle bottom
                     
                }
               
            }
            
            if (rounded)
            {
                _popupFramer = _popupFrame;
            }
            else
            {
                _popupFramer = _popupStack; 
                 
            }
            #endregion
        }


        public void ShowPopupCenter(View view, double scale = .80, string title = "", bool modal = true, bool rounded = false)
        {
            
            _popupType = PopupType.Center;
            if (scale <= 1 && scale >= .25)
            {
                _scale = scale;
            }
            else
            {
                _scale = .80;
            }
            _isModal = modal;
            _popupContent = view;
            PopupChanged = false;

            _popupWidth = _parentPage.Width * _scale;
            _popupHeight = _parentPage.Height * _scale;
      
            DrawPopup(view, rounded, title);
            _popupLeft = ((_parentPage.Width - _parentPage.Padding.HorizontalThickness) - _popupWidth) / 2;
            _popupTop = ((_parentPage.Height - _parentPage.Padding.VerticalThickness) - _popupHeight) / 2;

            SetLayoutFlags(_popupFramer, AbsoluteLayoutFlags.None);
            SetLayoutBounds(_popupFramer, new Rectangle(_popupLeft + _popupWidth / 2, _popupTop + _popupHeight / 2, 0, 0));


            if (_isModal)
            {
                _popupShield = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = new Color(0, 0, 0, 0.4)
                };
                SetLayoutFlags(_popupShield, AbsoluteLayoutFlags.None);
                Children.Add(_popupShield, new Rectangle(0, 0, _parentPage.Width, _parentPage.Height));
            }
           
            Children.Add(_popupFramer);
            _popupFramer.LayoutTo(new Rectangle(_popupLeft, _popupTop, _popupWidth, _popupHeight), 150, Easing.Linear);
            if (!PopupVisible)
            {
                OnOpened(); 
            }

            PopupVisible = true;
             
        }

     

        public void ShowPopupRelative(View view, View relativeTo, double width, double height, bool modal, string title)
        {
            _popupContent = view;
            _relativeTo = relativeTo;
            _oheight = height;
            _owidth = width;
            PopupChanged = false;
            _height = _oheight + 16; 
            _width = _owidth;
            _isModal = modal;
            _popupType = PopupType.Relative;
            _title = title;
            var points = this.GetXY(_relativeTo);
            DrawPopup(_popupContent, false, _title);
            if (_isModal)
            {
                _popupShield = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = new Color(0, 0, 0, 0.4)
                };
                SetLayoutFlags(_popupShield, AbsoluteLayoutFlags.None);
                Children.Add(_popupShield, new Rectangle(0, 0, _parentPage.Width, _parentPage.Height));
            }
            SetLayoutFlags(_popupFramer, AbsoluteLayoutFlags.None);
            if (!PopupVisible)
            {
                SetLayoutBounds(_popupFramer, new Rectangle(points.X, points.Y, 0, 0));
            }
           
            Children.Add(_popupFramer); 
            _triangleStack.Children.Insert(0, new StackLayout{ BackgroundColor = Color.Transparent, WidthRequest = (GetX(relativeTo) - points.X) + (_relativeTo.Width / 2) - 10 });

            if (!PopupVisible)
            {
                _popupFramer.LayoutTo(new Rectangle(points.X, points.Y, _width, _height), Device.OnPlatform<uint>(150, 50, 50), Easing.Linear);
                OnOpened(); 
            }
            else
            {
               
                SetLayoutBounds(_popupFramer, new Rectangle(points.X, points.Y, _width, _height));
            }

            PopupVisible = true;
        }

        public void ShowPopupByMargin(View view, Rectangle margins, bool modal, bool rounded, string title)
        {
             
            _popupContent = view;
            PopupChanged = false;
            
            _isModal = modal;
            _popupType = PopupType.Margin;
            _marginRect = margins;
            _left = _parentPage.Width * margins.Left;
             
            _top = _parentPage.Height * margins.Top;
           
            _right = _parentPage.Width * margins.Right;
            _bottom = _parentPage.Height * margins.Bottom;
            DrawPopup(view, rounded, title);
            SetLayoutFlags(_popupFramer, AbsoluteLayoutFlags.None);
            SetLayoutBounds(_popupFramer, new Rectangle(_left, _top, _parentPage.Width - _right, 0));
            if (_isModal)
            {
                _popupShield = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = new Color(0, 0, 0, 0.4)
                };
                SetLayoutFlags(_popupShield, AbsoluteLayoutFlags.None);
                Children.Add(_popupShield, new Rectangle(0, 0, _parentPage.Width, _parentPage.Height));
            }
            
           
            Children.Add(_popupFramer);
            _popupFramer.LayoutTo(new Rectangle(_left, _top, _parentPage.Width - _right, _parentPage.Height - _bottom), Device.OnPlatform<uint>(150, 50, 50), Easing.Linear);
            if (!PopupVisible)
            {
                OnOpened(); 
            }

            PopupVisible = true;
        }
    }
}





