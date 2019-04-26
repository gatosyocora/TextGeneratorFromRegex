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

namespace Regex2DFA
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MAX_REPEAT_CHAR_NUM = 5;

        public string RegexText;

        public string ResultText;
        
        Random r;

        private static readonly string ALPHABET_SMALL_CHARS = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string ALPHABET_LARGE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string NUM_CHARS = "0123456789";

        private static readonly string ALL_CHARS = ALPHABET_SMALL_CHARS + ALPHABET_LARGE_CHARS + NUM_CHARS;

        public MainWindow()
        {
            InitializeComponent();

            RegexText = "";
            ResultText = "result";

            r = new Random(1000);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RegexText = ((TextBox)sender).Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(RegexText);
            ResultTextBox.Text = RegexPatterns(RegexText);
        }

        private string RegexPatterns(string regex)
        {
            string resultWord = "";

            string preWord = "";


            char nowChar;
            for (int i = 0; i < regex.Length; i++)
            {
                nowChar = regex[i];

                // TODO: 入れ子への対応
                // TODO: (|)での選択に対応
                if (nowChar.Equals('('))
                {
                    resultWord += preWord;
                    preWord = "";

                    string partRegex = "";
                    while (true)
                    {
                        i++;

                        if (i >= regex.Length)
                        {
                            Console.Write("(と)の数が一致しません");
                            return "(と)の数が一致しません";
                        }

                        nowChar = regex[i];
                        if (nowChar.Equals(')'))
                        {
                            preWord += RegexPatterns(partRegex);
                            break;
                        }
                        else
                        {
                            partRegex += nowChar;
                        }
                    }
                }
                else if (nowChar.Equals('['))
                {
                    string wordList = "";
                    bool isNot = false;

                    while (true)
                    {
                        i++;

                        if (i >= regex.Length)
                        {
                            Console.Write("[と]の数が一致しません");
                            return "[と]の数が一致しません";
                        }

                        nowChar = regex[i];
                        if (nowChar.Equals(']'))
                        {
                            if (isNot)
                            {
                                string addWordList = ALL_CHARS.ToString();

                                foreach (var c in wordList)
                                {
                                    addWordList = addWordList.Replace(c + "", "");
                                }
                                preWord = addWordList[r.Next(addWordList.Length)] + "";
                            }
                            else
                                preWord += wordList[r.Next(wordList.Length)] + "";
                            break;
                        }
                        else if (nowChar.Equals('^'))
                        {
                            isNot = true;
                        }
                        else
                        {
                            wordList += nowChar;
                        }
                    }
                }
                // TODO: {}?にも対応し区別をつける
                else if (nowChar.Equals('{'))
                {
                    string maxLoopNum = "";
                    string minLoopNum = "";
                    while (true)
                    {
                        i++;

                        if (i >= regex.Length)
                        {
                            Console.Write("{と}の数が一致しません");
                            return "{と}の数が一致しません";
                        }

                        nowChar = regex[i];
                        if (nowChar.Equals('}'))
                        {
                            var loopNum = (minLoopNum == "") ? int.Parse(maxLoopNum) : r.Next(int.Parse(minLoopNum), int.Parse(maxLoopNum));

                            resultWord += string.Concat(Enumerable.Repeat(preWord, loopNum));
                            preWord = "";

                            break;
                        }
                        else if (nowChar.Equals(','))
                        {
                            minLoopNum = maxLoopNum;
                            maxLoopNum = "";
                        }
                        else if (char.IsNumber(nowChar))
                        {
                            maxLoopNum += nowChar;
                        }
                        else
                        {
                            Console.Write("{}の中に数字以外が含まれています");
                            return "{}の中に数字以外が含まれています";
                        }
                    }
                }
                // TODO: .+や.*への対応
                // TODO: アルファベットと数字以外とも一致するように
                else if (nowChar.Equals('.'))
                {
                    resultWord += preWord;
                    resultWord += ALL_CHARS[r.Next(ALL_CHARS.Length)];
                    preWord = "";
                }
                else if (nowChar.Equals('*'))
                {
                    if (preWord.Equals("")) continue;

                    resultWord += string.Concat(Enumerable.Repeat(preWord, r.Next(MAX_REPEAT_CHAR_NUM)));
                    preWord = "";
                }
                else if (nowChar.Equals('+'))
                {
                    if (preWord.Equals("")) continue;

                    resultWord += string.Concat(Enumerable.Repeat(preWord, r.Next(1, MAX_REPEAT_CHAR_NUM)));
                    preWord = "";
                }
                else if (nowChar.Equals('?'))
                {
                    resultWord += (r.Next(2) == 1) ? preWord : "";
                    preWord = "";
                }
                else if (nowChar.Equals('\\'))
                {
                    resultWord += preWord;
                    preWord = "";

                    i++;

                    if (i >= regex.Length)
                    {
                        Console.Write("最後が\\になってはいけません");
                        return "最後が\\になってはいけません";
                    }

                    nowChar = regex[i];

                    if (nowChar.Equals('w'))
                    {
                        string CHARS = ALL_CHARS + "_";
                        resultWord += CHARS[r.Next(CHARS.Length)];
                    }
                    else if (nowChar.Equals('W'))
                    {
                        // アルファベット、数字、アンダスコア以外の1文字らしいけど有効な文字の集合がわからん
                        string CHARS = ALL_CHARS + "_";
                        resultWord += CHARS[r.Next(CHARS.Length)];
                    }
                    else if (nowChar.Equals('d'))
                    {
                        resultWord += NUM_CHARS[r.Next(NUM_CHARS.Length)];
                    }
                    else if (nowChar.Equals('D'))
                    {
                        string CHARS = ALPHABET_LARGE_CHARS + ALPHABET_SMALL_CHARS;
                        resultWord += CHARS[r.Next(CHARS.Length)];
                    }
                    else if (nowChar.Equals('s'))
                    {
                        string CHARS = " \t\n";
                        resultWord += CHARS[r.Next(CHARS.Length)];
                    }
                    else if (nowChar.Equals('S'))
                    {
                        // 半角スペース、タブ、改行以外の1文字らしいけど有効な文字の集合がわからん
                        string CHARS = ' '+'\t'+'\n'+"";
                        resultWord += CHARS[r.Next(CHARS.Length)];
                    }
                    else if (nowChar.Equals('n'))
                    {
                        resultWord += '\n';
                    }
                    else if (nowChar.Equals('t'))
                    {
                        resultWord += '\t';
                    }
                    else if (nowChar.Equals('\\'))
                    {
                        resultWord += '\\';
                    }


                }
                else
                {
                    resultWord += preWord;
                    preWord = nowChar+"";
                }
            }

            resultWord += preWord;

            return resultWord;
        }
    }
}
