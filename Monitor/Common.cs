using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Monitor
{
        public enum User {UNKNOWN,ADMIN}
        public class Common:INotifyPropertyChanged
        {
                public event PropertyChangedEventHandler PropertyChanged;//当属性发生改变时触发事件通知绑定对象
                private User userLevel;
                public User UserLevel 
                {
                        get{return userLevel;}
                        set
                        {
                                userLevel = value;
                                if (PropertyChanged != null)
                                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("UserLevel"));
                        }
                }

                public static ComType ComType;
                public static int SelectedAddress = -1;
        }
}
