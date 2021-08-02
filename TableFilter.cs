using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NetSnake
{
    //*************************************************************************************
    //* Данный класс реализует модуль отображения пакетов по заданным параметрам филльтра *
    //*************************************************************************************
    class TableFilter
    {
        private bool checkParams = false;
        private const int numOfProtColmn = 6;
        private const int offsetFlags = 2;//поскольку коллекция флагов начинается с 3 колонки таблицы данных пакетов
        private TextBox filterBox;
        private bool IsOrConstructions = false;
        private List<string[]> allPacketsTable;
        private List<string[]> filteredTable;
        private DataGridView viwerPacketsTable;
        private string[] parms;
        private string[] flags = { "ipsource", "portsource", "ipdest", "portdest", "prot", "length" };
        public TableFilter(
            TextBox filterBox,
            List<string[]> allPacketsTable,
            DataGridView viwerPacketsTable
            )
        {
            this.filterBox = filterBox;
            this.allPacketsTable = allPacketsTable;
            this.filteredTable = new List<string[]>();
            this.viwerPacketsTable = viwerPacketsTable;

            filterBox.TextChanged += new EventHandler(TextChanged);
            filterBox.KeyDown += new KeyEventHandler(EnterKeyPressed);
        }

        ///<summary>
        ///Добавление строк в коллецию, соотвествующую требованиям для OR
        ///</summary>
        private List<string[]> AddFilteredRows(string rowValue, int index, List<string[]> unfilteredTable, bool isOrCombo)
        {

            for (int i = 0; i < allPacketsTable.Count; i++)
            {
                if (allPacketsTable[i][index] == rowValue)
                {
                    bool check = true;
                    for (int j = 0; j < unfilteredTable.Count; j++)
                    {
                        if (unfilteredTable[j][0] == allPacketsTable[i][0])
                        {
                            check = false;
                            break;
                        }
                    }

                    if (check)
                        filteredTable.Add(allPacketsTable[i]);
                }


            }
            return filteredTable;
        }

        ///<summary>
        ///Добавление строк в коллецию, соотвествующую требованиям
        ///</summary>
        private List<string[]> AddFilteredRows(string rowValue, int index, List<string[]> unfilteredTable)
        {
            List<string[]> filteredTable = new List<string[]>();
            for (int i = 0; i < unfilteredTable.Count; i++)
            {
                if (unfilteredTable[i][index] == rowValue)
                {
                    filteredTable.Add(unfilteredTable[i]);
                }
            }
            return filteredTable;
        }

        ///<summary>
        ///Отбор пакетов из таблицы, соответствующих указанным параметрам
        ///</summary>
        private string[] DeleteAllBackSpaces(string[] strWithSpaces)
        {
            string[] strWithOutSpaces = new string[strWithSpaces.Length];
            for (int i = 0; i < strWithSpaces.Length; i++)
            {
                strWithOutSpaces[i] = strWithSpaces[i].Trim(' ');

            }
            return strWithOutSpaces;
        }

        ///<summary>
        ///Отбор пакетов из таблицы, соответствующих указанным параметрам
        ///</summary>
        private void SelectPacketsByParams()
        {
            if (IsOrConstructions)
                filteredTable = new List<string[]>();
            else filteredTable = allPacketsTable;

            for (int i = 0; i < parms.Length; i++)
            {
                string[] param = DeleteAllBackSpaces(parms[i].Split('='));
                for (int j = 0; j < flags.Length; j++)
                {
                    if (flags[j] == param[0])
                    {
                        //если комбинация параметров 'или'
                        if (IsOrConstructions)
                        {
                            filteredTable = AddFilteredRows(param[1], j + offsetFlags, filteredTable, IsOrConstructions);
                        }
                        //если комнбиация фильтров-и либо без комбинации
                        else filteredTable = AddFilteredRows(param[1], j + offsetFlags, filteredTable);

                        break;
                    }
                }
            }

        }

        ///<summary>
        ///При нажатии enter начинает применять фильтр
        ///</summary>
        private void EnterKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (MonitorPackets.IsActiveCapture == false)
                {
                    if (checkParams)
                    {
                        viwerPacketsTable.Rows.Clear();
                        SelectPacketsByParams();
                        for (int rowIndex = 0; rowIndex < filteredTable.Count; rowIndex++)
                        {
                            viwerPacketsTable.Rows.Add(filteredTable[rowIndex]);
                            viwerPacketsTable.Rows[rowIndex].DefaultCellStyle.BackColor = GetRowColor(filteredTable[rowIndex][numOfProtColmn]);
                        }
                    }
                }
                else MessageBox.Show(
                    "FilterPackets can be used only with stopCapture!",
                    "Warning!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                    );
            }
        }

        ///<summary>
        ///Прверяет введеный параметр на соответсвие ключевому слову
        ///</summary>
        private bool CheckKeyWords(string text)
        {

            for (int i = 0; i < flags.Length; i++)
            {
                if (text.Contains(flags[i]))
                    return true;
            }
            return false;

        }

        ///<summary>
        ///Определяет цвет строки
        ///</summary>
        private Color GetRowColor(string protocol)
        {
            switch (protocol)
            {
                case "ICMPV6":
                    return Color.LightYellow;
                case "TCP":
                    return Color.Aquamarine;
                case "UDP":
                    return Color.LightSalmon;
                case "ARP":
                    return Color.LightPink;
            }
            return Color.White;


        }
        ///<summary>
        ///Вызывается при изменении текста
        ///</summary>
        private void TextChanged(object semder, EventArgs e)
        {
            filterBox.BackColor = CheckCorrectFlags(filterBox.Text);
        }

        ///<summary>
        ///Устанавливает цвет поля ввода параметра 
        ///</summary>
        private Color CheckCorrectFlags(string text)
        {
            string[] comboOr = text.Split('|');
            string[] comboAnd = text.Split('&');

            //если наш параметр фильтрации состоит из нескольких 'или'
            if (comboOr.Length > 1)
            {
                IsOrConstructions = true;
                for (int i = 0; i < comboOr.Length; i++)
                {
                    if (!CheckKeyWords(comboOr[i]))
                    {
                        checkParams = false;
                        return Color.LightPink;
                    }
                }

                checkParams = true;
                parms = comboOr;
                return Color.LightGreen;
            }
            //если наш параметр фильтрации состоит из нескольких 'и'
            else if (comboAnd.Length > 1)
            {
                IsOrConstructions = false;
                for (int i = 0; i < comboAnd.Length; i++)
                {
                    if (!CheckKeyWords(comboAnd[i]))
                    {
                        checkParams = false;
                        return Color.LightPink;
                    }
                }
                parms = comboAnd;
                checkParams = true;
                return Color.LightGreen;
            }

            //если наш параметрс фильтрации одиночный
            else
            {
                IsOrConstructions = false;
                //если синтаксис параметра верный-цвет поля зеленый
                if (CheckKeyWords(text))
                {
                    parms = new string[] { text };
                    checkParams = true;
                    return Color.LightGreen;

                }

                //если поле пустое- цвет поля зеленый
                else if (text.Length == 0)
                {
                    checkParams = false;
                    viwerPacketsTable.Rows.Clear();
                    for (int i = 0; i < allPacketsTable.Count; i++)
                    {
                        viwerPacketsTable.Rows.Add(allPacketsTable[i]);
                        viwerPacketsTable.Rows[i].DefaultCellStyle.BackColor = GetRowColor(allPacketsTable[i][numOfProtColmn]);
                    }
                    return Color.White;

                }
                //если синтаксис параметра неверный- цвет поля розовый
                else
                {
                    checkParams = false;
                    return Color.LightPink;
                }
            }


        }
    }
}
