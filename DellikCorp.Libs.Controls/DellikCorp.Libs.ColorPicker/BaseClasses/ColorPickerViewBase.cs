﻿using DellikCorp.Libs.ColorPicker.BaseClasses.ColorPickerEventArgs;
using DellikCorp.Libs.ColorPicker.Interfaces;
using Microsoft.Maui.Controls.Compatibility;

namespace DellikCorp.Libs.ColorPicker.BaseClasses
{
    public abstract class ColorPickerViewBase : Layout<View>, IColorPicker
    {
        public event EventHandler<ColorChangedEventArgs> SelectedColorChanged;

        public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
           nameof(SelectedColor),
           typeof(Color),
           typeof(IColorPicker),
           Color.FromHsla(0, 0, 0.5),
           propertyChanged: HandleSelectedColorSet);

        static void HandleSelectedColorSet(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                ((ColorPickerViewBase)bindable).ChangeSelectedColor((Color)newValue);
                if (((ColorPickerViewBase)bindable).ConnectedColorPicker != null)
                {
                    ((ColorPickerViewBase)bindable).ConnectedColorPicker.SelectedColor = (Color)newValue;
                }
                ((ColorPickerViewBase)bindable).RaiseSelectedColorChanged((Color)oldValue, (Color)newValue);
            }
        }

        public Color SelectedColor
        {
            get
            {
                return (Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public static readonly BindableProperty ConnectedColorPickerProperty = BindableProperty.Create(
           nameof(ConnectedColorPicker),
           typeof(IColorPicker),
           typeof(IColorPicker),
           null,
           propertyChanged: HandleConnectedColorPickerSet);

        static void HandleConnectedColorPickerSet(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != null)
            {
                ((IColorPicker)oldValue).PropertyChanged -= ((ColorPickerViewBase)bindable).BindedIColorPicker_PropertyChanged;
            }
            if (newValue != null)
            {
                ((IColorPicker)newValue).PropertyChanged += ((ColorPickerViewBase)bindable).BindedIColorPicker_PropertyChanged;
                ((IColorPicker)newValue).SelectedColor = ((ColorPickerViewBase)bindable).SelectedColor;
            }
        }

        public IColorPicker ConnectedColorPicker
        {
            get
            {
                return (IColorPicker)GetValue(ConnectedColorPickerProperty);
            }
            set
            {
                SetValue(ConnectedColorPickerProperty, value);
            }
        }

        protected abstract void ChangeSelectedColor(Color color);

        private void BindedIColorPicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedColor))
            {
                SelectedColor = ((IColorPicker)sender).SelectedColor;
            }
        }

        protected virtual void RaiseSelectedColorChanged(Color oldColor, Color newColor)
        {
            SelectedColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
        }
    }
}
