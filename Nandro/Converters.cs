using Avalonia.Data.Converters;
using Avalonia.Media;
using Nandro.Nano;
using System;
using System.Globalization;

namespace Nandro
{
    public class TestResultToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var testResult = (EndpointTestResult)value;
            switch (testResult)
            {
                case EndpointTestResult.Success:
                    return Brushes.Green;
                case EndpointTestResult.Fail:
                    return Brushes.Red;
                case EndpointTestResult.Pending:
                    return Brushes.Black;
                default:
                    throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestResultNotPendingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var testResult = (EndpointTestResult)value;
            return testResult != EndpointTestResult.Pending;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
