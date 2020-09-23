using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.ML;
using Microsoft.ML.Data;
namespace ADNMenuSample
{
    /// <summary>
    /// Interaction logic for ApplicationView.xaml
    /// </summary>
    public partial class ApplicationView : Window
    {
        public ApplicationView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string modelLocation = @"D:\model.onnx";
                /*MLContext mlContext = new MLContext();
                                string modelLocation = "/ADNMenuSample;Component/model.zip";

                Uri uri = new Uri(modelLocation, UriKind.Relative);
                System.Windows.Resources.StreamResourceInfo info = Application.GetResourceStream(uri);
                ITransformer trainedModel = mlContext.Model.Load(info.Stream, out DataViewSchema modelSchema);*/
                MLContext mlContext = new MLContext();
                var data = mlContext.Data.LoadFromEnumerable(new List<ImageData>());
                var pipeline = mlContext.Transforms.LoadImages(outputColumnName: "image", imageFolder: "", inputColumnName: nameof(ImageData.ImagePath))
                        .Append(mlContext.Transforms.ResizeImages(outputColumnName: ModelSettings.ModelInput, imageWidth: ImageResNetSettings.imageWidth, imageHeight: ImageResNetSettings.imageHeight, inputColumnName: "image"))
                        .Append(mlContext.Transforms.ExtractPixels(outputColumnName: ModelSettings.ModelInput))
                        .Append(mlContext.Transforms.CustomMapping(new NormalizeMapping().GetMapping(), contractName: nameof(NormalizeMapping)))
                        .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { ModelSettings.ModelOutput }, inputColumnNames: new[] { ModelSettings.ModelInput }));
                var model = pipeline.Fit(data);
                mlContext.ComponentCatalog.RegisterAssembly(typeof(NormalizeMapping).Assembly);
                IEnumerable<ImageData> image = ImageData.ReadImageFromPaths(new string[] { @"E:\test.jpg" });
                IDataView imageDataView = mlContext.Data.LoadFromEnumerable(image);
                IDataView scoredData = model.Transform(imageDataView);
                List<float[]> probabilities = scoredData.GetColumn<float[]>(ModelSettings.ModelOutput).ToList();
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        public class ImageData
        {
            [Microsoft.ML.Data.LoadColumn(0)]
            public string ImagePath;

            [Microsoft.ML.Data.LoadColumn(1)]
            public string Label;
            public ImageData()
            {

            }
            public ImageData(string imagePath)
            {
                ImagePath = imagePath;
            }
            public static IEnumerable<ImageData> ReadFromFile(string imageFolder)
            {
                return Directory
                    .GetFiles(imageFolder)
                    .Where(filePath => System.IO.Path.GetExtension(filePath) != ".md")
                    .Select(filePath => new ImageData { ImagePath = filePath, Label = System.IO.Path.GetFileName(filePath) });
            }
            public static IEnumerable<ImageData> ReadImageFromPaths(string[] imagePaths)
            {
                return imagePaths.Select(filePath => new ImageData { ImagePath = filePath, Label = System.IO.Path.GetFileName(filePath) });
            }
        }
        public struct ImageResNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;             // (offsetImage: ImageSettingsForTFModel.mean)
            public const float scale = 1 / 255f;         //1/255f for InceptionV3. Not used for InceptionV1
            public const bool channelsLast = true;

        }
        public struct ModelSettings
        {

            // input tensor name
            public const string ModelInput = "input";

            // output tensor name
            public const string ModelOutput = "output";
        }
        [Microsoft.ML.Transforms.CustomMappingFactoryAttribute(nameof(NormalizeMapping))]
        public class NormalizeMapping : Microsoft.ML.Transforms.CustomMappingFactory<NormalizeInput, NormalizeOutput>
        {
            // This is the custom mapping. We now separate it into a method, so that we can use it both in training and in loading.
            public static void Mapping(NormalizeInput input, NormalizeOutput output)
            {
                var values = input.Reshape.GetValues().ToArray();

                var image_mean = new float[] { 0.5f, 0.5f, 0.5f };
                var image_std = new float[] { 0.5f, 0.5f, 0.5f };

                for (int x = 0; x < values.Count(); x++)
                {
                    var y = x % 3;
                    // Normalize by 255 first
                    values[x] /= 255;
                    values[x] = (values[x] - image_mean[y]) / image_std[y];
                };

                output.Reshape = new Microsoft.ML.Data.VBuffer<float>(values.Count(), values);
            }
            // This factory method will be called when loading the model to get the mapping operation.
            public override Action<NormalizeInput, NormalizeOutput> GetMapping()
            {
                return Mapping;
            }
        }
        public class NormalizeInput
        {
            [Microsoft.ML.Data.ColumnName("input")]
            [VectorType(3, 224, 224)]
            public VBuffer<float> Reshape;
        }
        public class NormalizeOutput
        {
            [ColumnName("output")]
            [VectorType(3 * 224 * 224)]
            public VBuffer<float> Reshape;
        }
    }


}
