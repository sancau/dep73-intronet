namespace DB73.Helpers
{
    using System;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using DB73.AdditionalViewModels;
    using System.IO;
    using System.Reflection;

    [ValueConversion(typeof(TreeNodeViewModel), typeof(ImageSource))]
    public class TreeNodeToImageConverter : IValueConverter
    {
        //private const string UriFormat = "\Resources\Images\{0
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var viewModel = value as TreeNodeViewModel;
                if (viewModel == null)

                     return Binding.DoNothing;

                // Folder image
                if (viewModel.Type == "Folder")
                    return new BitmapImage(new Uri(
                        Path.Combine
                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/Images/folder.png")));

                // Word image
                if (viewModel.DocType == "Документ Word")
                    return new BitmapImage(new Uri(
                        Path.Combine
                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/Images/Word.png")));

                // PDF image
                if (viewModel.DocType == "Документ PDF")
                    return new BitmapImage(new Uri(
                        Path.Combine
                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/Images/32/pdf.png")));

                // Excel image
                if (viewModel.DocType == "Документ Excel")
                    return new BitmapImage(new Uri(
                        Path.Combine
                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/Images/32/Excel.jpg")));

                // Text file
                if (viewModel.DocType == "Текстовый файл")
                    return new BitmapImage(new Uri(
                        Path.Combine
                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/Images/Text.png")));

                if (viewModel.DocType == "Графический файл")
                    return new BitmapImage(new Uri(
                        Path.Combine
                            (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "Resources/Images/32/Picture.png")));

                else return Binding.DoNothing;
            }
            catch(Exception)
            {
                return Binding.DoNothing;
            }
        }

        // no need in this member of IValueConverter
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
