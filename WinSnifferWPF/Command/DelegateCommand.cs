using System;
using System.Windows.Input;

namespace WinSnifferWPF.Command
{
    /// <summary>
    /// 用于绑定Command父类
    /// </summary>
    class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            if (ExcuteAction == null)
            {
                return;
            }
            ExcuteAction(parameter);
        }

        /// <summary>
        /// 执行的方法委托
        /// </summary>
        public Action<object> ExcuteAction { get; set; }
        public Func<object, bool> CanExecuteFunc { get; set; }
    }
}
