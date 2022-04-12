using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int count = 0;
        private List<TextBox> listTextBoxRegister = new List<TextBox>();
        private List<string> listStringRegister = new List<string>();
        private string textInput = "";
        private string textEncodingResult = "";
        private string textDecodingResult = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void textBoxCountAdd_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Создаем столько полей для ввода, сколько ввели количество сумматоров
            Int32.TryParse(textBoxCountAdd.Text, out count);
            stackPanelRegisters.Children.Clear();
            listTextBoxRegister.Clear();
            for (int i = 0; i < count; i++)
            {
                TextBox textBox = new TextBox();
                listTextBoxRegister.Add(textBox);
                stackPanelRegisters.Children.Add(textBox);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // В массив строк заносим введенные регистры сумматоров (например, 1,2,3 или 1,3)
            listStringRegister.Clear();
            foreach (TextBox tb in listTextBoxRegister)
            {
                listStringRegister.Add(tb.Text);
            }
            // Получаем из поля текст для кодирования
            textInput = textBoxInput.Text;
            // Кодируем сверточным кодом
            textEncodingResult = Encoded(listStringRegister, textInput);
            // Выводим полученный код в окошко
            textBoxEncodingResult.Text = textEncodingResult;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // В массив строк заносим введенные регистры сумматоров (например, 1,2,3 или 1,3)
            listStringRegister.Clear();
            foreach (TextBox tb in listTextBoxRegister)
            {
                listStringRegister.Add(tb.Text);
            }
            // Получаем из поля текст для декодирования
            textEncodingResult = textBoxEncodingResult.Text;
            // Декодируем, используя последовательный алгоритм
            textDecodingResult = Decoded(listStringRegister, textEncodingResult);
            // Выводим декодированный текст в окошко
            textBoxDecodingResult.Text = textDecodingResult;
        }

        // Метод, производящий кодирование
        private string Encoded(List<string> listStringRegister, string textInput)
        {
            // Превращаем введенную строку, например, "qw" в коды согласно кодировке UTF8,
            // а затем - из кодов в двоичную систему счисления.
            // В итоге получаем последовательность нулей и единиц
            List<int> textInputBin = ConvertTextToBin(textInput);

            // Превращаем введенные регистры сумматоров в массив нулей и единиц
            // Например, 1,2,3 превратится в [1,1,1]
            //           1,3 превратится в   [1,0,1]
            List<int[]> registresAdder = ConvertRegistersToBin(listStringRegister);

            int[] registers = new int[3] { 0, 0, 0 };
            string result = "";

            // Каждое очередное число во входной последовательности (это может быть 0 или 1)
            // добавляем в регистр (по условию он 3-разрядный) со сдвигом вправо.
            // Для каждого сумматора вычисляем XOR нужных ячеек регистра и добавляем к выходной последовательности            
            foreach (int t in textInputBin)
            {
                registers = ShiftRight(registers, t);
                foreach (int[] r in registresAdder)
                {
                    int xorResult = GetXorResult(registers, r);
                    result += xorResult.ToString();
                }
            }
            return result;
        }

        // Метод, производящий декодирование
        private string Decoded(List<string> listStringRegister, string textEncodingResult)
        {
            // Превращаем введенные регистры сумматоров в массив нулей и единиц
            // Например, 1,2,3 превратится в [1,1,1]
            //           1,3 превратится в   [1,0,1]
            List<int[]> registresAdder = ConvertRegistersToBin(listStringRegister);
            // Текущее состояние регистра
            int[] currentState = new int[3] { 0, 0, 0 };
            string result = "";

            // Считываем цифры из строки, которую нужно декодировать по count штук за раз. Count - количество сумматоров
            for (int i = 0; i < textEncodingResult.Length / count; i++)
            {
                string symbolToDecode = textEncodingResult.Substring(i * count, count);
                // Декодируем очередную цифру в зависимости от состояния регистра и добавляем ее к результату
                int d = DecodeOneDigit(symbolToDecode, currentState, registresAdder);
                currentState = ShiftRight(currentState, d);
                result += currentState[0].ToString();
            }
            // Превращаем двоичный код в обычный текст, используя кодировку UTF8
            return ConvertBinToText(result);
        }

        private int GetXorResult(int[] registers, int[] registersAdder)
        {
            int result = 0;
            for (int i = 0; i < 3; i++)
            {
                if (registersAdder[i] == 1)
                {
                    result = result ^ registers[i];
                }
            }
            return result;
        }

        private List<int> ConvertTextToBin(string textInput)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(textInput);
            string tempString = "";
            foreach (byte b in bytes)
            {
                tempString += Convert.ToString(b, 2).PadLeft(8, '0');
            }
            List<int> result = new List<int>();
            foreach (char c in tempString)
            {
                result.Add(Convert.ToInt32(c.ToString()));
            }
            return result;
        }

        private string ConvertBinToText(string binString)
        {
            byte[] result = new byte[binString.Length / 8];
            for (int i = 0; i < binString.Length / 8; i++)
            {
                result[i] = (byte)Convert.ToInt32(binString.Substring(i * 8, 8), 2);
            }
            return Encoding.UTF8.GetString(result);
        }

        private List<int[]> ConvertRegistersToBin(List<string> listStringRegister)
        {
            List<int[]> result = new List<int[]>();
            for (int i = 0; i < listStringRegister.Count; i++)
            {
                int[] arrayRegister = new int[3] { 0, 0, 0 };
                if (listStringRegister[i].Contains("1"))
                {
                    arrayRegister[0] = 1;
                }
                if (listStringRegister[i].Contains("2"))
                {
                    arrayRegister[1] = 1;
                }
                if (listStringRegister[i].Contains("3"))
                {
                    arrayRegister[2] = 1;
                }
                result.Add(arrayRegister);
            }
            return result;
        }

        private int DecodeOneDigit(string symbolToDecode, int[] currentState, List<int[]> registresAdder)
        {
            int[] currentStateV1 = ShiftRight(currentState, 0);
            int[] currentStateV2 = ShiftRight(currentState, 1);
            string v1 = "";
            string v2 = "";
            foreach (int[] registers in registresAdder)
            {
                int xorResult = GetXorResult(currentStateV1, registers);
                v1 += xorResult.ToString();
            }
            foreach (int[] registers in registresAdder)
            {
                int xorResult = GetXorResult(currentStateV2, registers);
                v2 += xorResult.ToString();
            }
            if (v1 == symbolToDecode)
                return 0;
            else if (v2 == symbolToDecode)
                return 1;
            else return Convert.ToInt32(symbolToDecode[0].ToString());
        }

        private int[] ShiftRight(int[] currentState, int v)
        {
            int[] result = new int[3];
            currentState.CopyTo(result, 0);
            result[2] = result[1];
            result[1] = result[0];
            result[0] = v;
            return result;
        }
    }
}