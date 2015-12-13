using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Concordance
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        int numberOfWords = 0;

        private void btn_Open_Click(object sender, EventArgs e)
        {
            // creating open dialog
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = ".txtFile|*.txt";
            dialog.Title = "Select text file";

            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                InputBox.Text = String.Empty; //reset
                try
                {
                    InputBox.Text = File.ReadAllText(dialog.FileName, Encoding.Default);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count > 0)
            {
                int maxLeftLength=0;

                //calculating maximum length
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].SubItems[1].Text.Length > maxLeftLength)
                        maxLeftLength = listView1.Items[i].SubItems[1].Text.Length;
                }

                // creating save dialog
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Filter = ".txt File|*.txt";
                dialog.Title = "Save TXT file";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    List<string> lines = new List<string>();

                    //adding some commentaries
                    lines.Add(string.Format("# Compiled with {0} at {1}",Program.name, DateTime.Now.ToString("H:mm dd/mm/yyyy")));
                    lines.Add("# Number of included words on left and right is: " + numberOfWords);
                    
                    lines.Add(string.Format("{0,-" + maxLeftLength + "}\t{1,-10}\t{2}", "LEFT CCONTEXT", "WORD", "RIGHT CONTEXT"));

                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        //adding item information
                        string itemStr = string.Format("{0," + maxLeftLength + "}\t{1,-10}\t{2}",
                            listView1.Items[i].SubItems[1].Text, 
                            listView1.Items[i].SubItems[2].Text,
                            listView1.Items[i].SubItems[3].Text);
                        lines.Add(itemStr);
                    }

                    File.WriteAllLines(dialog.FileName, lines.ToArray());

                    MessageBox.Show("Operation completed!", Program.name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else MessageBox.Show("Nothing to save!", Program.name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputBox.Text))
            {
                if (string.IsNullOrWhiteSpace(textBox_word.Text))
                {
                    MessageBox.Show("Word field is empty!", Program.name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string word = (checkBox_IgnoreCase.Checked) ? textBox_word.Text.ToLower() : textBox_word.Text;

                listView1.Items.Clear();//reset

                //splitting to paragraphs to avoid mixing contexts from different paragraphs
                string[] paragraphs = InputBox.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                // tracking variables
                int listViewItemIndex = 1;
                numberOfWords = (int)numericUpDown1.Value;

                //output variables
                string left = string.Empty;//left context
                string right = string.Empty;//right context

                for (int i = 0; i < paragraphs.Length; i++)
                {
                    //all words in the paragraph
                    string[] wordsInParagraph = paragraphs[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    //words with no punctuation (for comparing only)
                    string[] wordsForComparing = RemovePunctuationNSplit(
                        (checkBox_IgnoreCase.Checked) ? paragraphs[i].ToLower() : paragraphs[i]);

                    if (wordsForComparing.Contains(word))
                    {
                        #region algorithm
                        for (int k = 0; k < wordsInParagraph.Length; k++)
                        {
                            //getting the context parts on left and right sides
                            //if there is a match
                            if (wordsForComparing[k] == word)
                            {
                                for (int currentIndex = 1; currentIndex <= numberOfWords; currentIndex++)
                                {
                                    //compensating the index out of range issues
                                    if (k - currentIndex >= 0 && k - currentIndex < wordsInParagraph.Length)
                                    {
                                        //creating left context
                                        left = wordsInParagraph[k - currentIndex] + left;
                                        if (currentIndex != numberOfWords) left = " " + left;//adding space to every but first 
                                    }

                                    //compensating the index out of range issues
                                    if (k + currentIndex >= 0 && k + currentIndex < wordsInParagraph.Length)
                                        //creating right context 
                                        right += wordsInParagraph[k + currentIndex] + ((currentIndex != numberOfWords) ? " " : string.Empty);


                                }

                                //output the info to listview
                                AddItemToListView(listViewItemIndex, left, wordsInParagraph[k], right);
                                listViewItemIndex++;

                                left = right = string.Empty;//reset

                            }// end if

                        }//end for
                        #endregion

                    }//end if

                }//end for

            }
            else MessageBox.Show("There is no text!", Program.name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
        }

        private void AddItemToListView(int id, string left, string word, string right)
        {
            ListViewItem item = new ListViewItem(id.ToString());
            item.SubItems.Add(left);
            item.SubItems.Add(word);
            item.SubItems.Add(right);
            listView1.Items.Add(item);
        }

        private string[] RemovePunctuationNSplit(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '’' || text[i] == '\'' || text[i] == '-')
                {
                    //keeping apostrophe sign inplace
                }
                else if (char.IsPunctuation(text[i]) || text[i] == '\"')
                {
                    text = text.Remove(i, 1);
                    i--;//to avoid messing up with the length as we removed one character
                }
            }
            return text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

    }
}
