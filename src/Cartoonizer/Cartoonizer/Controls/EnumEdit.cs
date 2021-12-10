using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Cartoonizer {
    public class EnumEdit : Control {
        #region Dependency Properties

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(EnumEdit), null);
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(IEnumerable), typeof(EnumEdit), null);
        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.Register("ValueType", typeof(Type), typeof(EnumEdit), new PropertyMetadata(new PropertyChangedCallback(OnValueTypeChanged)));

        private static void OnValueTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e) {
            ((EnumEdit)o).OnValueTypeChanged();
        }

        #endregion Dependency Properties

        public EnumEdit() {
            DefaultStyleKey = typeof(EnumEdit);
        }

        public object Value {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public IEnumerable Values {
            get { return (IEnumerable)GetValue(ValuesProperty); }
            private set { SetValue(ValuesProperty, value); }
        }
        public Type ValueType {
            get { return (Type)GetValue(ValueTypeProperty); }
            set { SetValue(ValueTypeProperty, value); }
        }

        protected virtual void OnValueTypeChanged() {
            UpdateValues();
        }
        protected void UpdateValues() {
            Values = Enum.GetValues(ValueType);
        }
    }
}
