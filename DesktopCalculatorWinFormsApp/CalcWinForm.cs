using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using Math;
using C1.Win.C1Input;
using C1.Win.C1Command;
using System.Diagnostics;

namespace DesktopCalculatorWinFormsApp
{
    // WinForms App for Desktop Calculator Application
    public partial class CalcWinForm : Form
    {
        double result = 0;
        double? operator1 = null;
        string operation = null;
        bool operationPerformed = false;

        C1MainMenu mainMenu;
        C1Label operationStack;
        C1TextBox resultBox;
        C1CommandHolder ch;
        MinimalSplitButton btnMR;
        SplitContainer splitContainer;
        TableLayoutPanel tableLayoutPanel;
        ToolTip toolTip;
        ResourceManager keyBinds;
        Evaluator eval;

        public CalcWinForm()
        {
            InitializeComponent();
            InitializeControls();
        }

        // To initialize custom defined controls of desktop calculator app
        private void InitializeControls()
        {
            toolTip = new();
            keyBinds = new(ProductName + ".KeyBinds", Assembly.GetExecutingAssembly());
            ch = C1CommandHolder.CreateCommandHolder(this);
            eval = new();

            AddFormStyles();
            AddSplitContainer();
            AddLabel();
            AddResultBox();
            AddMainMenu();
            AddTable();
            AddButtons();
        }

        // Form properties
        private void AddFormStyles()
        {
            Size = new(350, 462);
            MinimumSize = new(244, 323);
            Text = StringResource.FormText;
            KeyPreview = true;
            StartPosition = FormStartPosition.CenterScreen;
            KeyDown += KeyInput;
            Icon = new(@".\Calc.ico");
            SizeChanged += CalcWinForm_SizeChanged;
        }

        // Adjust size of diffrent controls when form resizes
        private void CalcWinForm_SizeChanged(object sender, EventArgs e)
        {
            int btnF = 12, resF = 28, lblF = 13, dist = 75; 
            bool isSmaller = (Size.Width < 280 || Size.Height < 400 && Size.Width >= MinimumSize.Width);
            bool isLarger = (Size.Width > 800 || Size.Height > 600);
            
            if (isSmaller || isLarger) // if the control size has become smaller or larger than the preferred size set sizes accordingly
            {
                btnF = isSmaller ? 9 : 18;
                lblF = isSmaller ? 11 : 20;
                resF = isSmaller ? 20 : 40;
                dist = isSmaller ? 50 : 140;
            }

            // Set font size for different control according to the client size
            foreach (Button btn in tableLayoutPanel.Controls)
                btn.Font = new Font(StringResource.FontName, btnF);
                splitContainer.SplitterDistance = dist;
            resultBox.Font = new Font(StringResource.FontName, resF);
            operationStack.Font = new Font(StringResource.FontName, lblF);
        }

        // Method to separate out output and input using a split panel
        private void AddSplitContainer()
        {
            splitContainer = new()
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Orientation = Orientation.Horizontal,
                FixedPanel = FixedPanel.Panel1,
                IsSplitterFixed = true,
                TabStop = false,
            };
            Controls.Add(splitContainer);
        }

        // Method to add previous calculations label to the form control
        private void AddLabel()
        {
            operationStack = new()
            {
                Font = new(StringResource.FontName, 13),
                Location = new(2, 0),
                Size = new(330, 22),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right,
            };
            splitContainer.Panel1.Controls.Add(operationStack);
        }

        // Method to add textbox to the form control
        private void AddResultBox()
        {
            resultBox = new()
            {
                Font = new(StringResource.FontName, 28),
                Location = new(7, 12),
                Size = new(325, 28),
                Text = StringResource.DefaultResult,
                ReadOnly = true,
                TabStop = false,
                BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Right,
                Anchor = AnchorStyles.Right,
            };
            splitContainer.Panel1.Controls.Add(resultBox);
            splitContainer.SplitterDistance = 75;
        }

        // Method to add menu items to the form
        private void AddMainMenu()
        {
            mainMenu = new();
            Controls.Add(mainMenu);
            foreach (string menuItem in StringResource.menuItems.Split(" | "))
            {
                C1CommandMenu newMenuItem = CreateMenuItem(menuItem);
                if (menuItem.Equals("Edit"))
                    foreach (string subItem in StringResource.editSubItems.Split(" | "))
                        newMenuItem.CommandLinks.Add(new C1CommandLink(CreateMenuItem(subItem)));
                mainMenu.CommandLinks.Add(new C1CommandLink(newMenuItem));
            }
        }

        // Method to create a C1 menu item
        private C1CommandMenu CreateMenuItem(string menuItem)
        {
            C1CommandMenu newMenuItem = ch.CreateCommand(typeof(C1CommandMenu)) as C1CommandMenu;
            newMenuItem.Text = menuItem;
            newMenuItem.Name = menuItem;
            newMenuItem.Click += MenuItem_Click;
            return newMenuItem;
        }

        // Method to add table layout panel for input buttons
        private void AddTable()
        {
            tableLayoutPanel = new()
            {
                ColumnCount = 5,
                RowCount = 7,
                Dock = DockStyle.Fill,
            };

            tableLayoutPanel.RowStyles.Add(new(SizeType.Percent, 9.09F));
            
            for (int row = 1; row < 7; row++)
                tableLayoutPanel.RowStyles.Add(new (SizeType.Percent, 15.15F));

            for (int col = 0; col < 5; col++)
                tableLayoutPanel.ColumnStyles.Add(new (SizeType.Percent, 20F));

            splitContainer.Panel2.Controls.Add(tableLayoutPanel);
        }

        // Method to add buttons to the form control
        private void AddButtons()
        {
            ResourceManager toolTips = new(ProductName + ".ToolTips", Assembly.GetExecutingAssembly());
            int prevTop = 0, prevLeft = 0, btnCount = 0;

            foreach (string item in StringResource.Operators.Split(" | "))
            {
                MinimalButton btn = new();
                ButtonStyles(btn, item, prevTop, prevLeft, toolTips);

                if (item != "MR") tableLayoutPanel.Controls.Add(btn, prevLeft, prevTop);
                else { ButtonStyles(btnMR = new(), item, prevTop, prevLeft, toolTips); tableLayoutPanel.Controls.Add(btnMR, prevLeft, prevTop); }

                // Button Location Logic for the next button
                btnCount++;
                prevLeft++;
                if (btnCount % 5 == 0)
                {
                    prevTop++;
                    prevLeft = 0;
                }
            }
        }

        // To style the buttons according to the calculator UI styles
        private void ButtonStyles(C1Button btn, string item, int prevTop, int prevLeft, ResourceManager toolTips)
        {
            btn.Text = item;
            btn.Name = StringResource.BtnName + item;
            btn.Size = new(70, prevTop == 0 ? 30 : 50);
            btn.Location = new(prevLeft, prevTop);
            btn.BackColor = int.TryParse(btn.Name[3..], out int temp) ? Color.Silver : Color.White;
            btn.MouseClick += Btn_Click;
            toolTip.SetToolTip(btn, toolTips.GetString(btn.Name));
        }

        // Handle what any button press will do
        private void Btn_Click(Object sender, EventArgs e)
        {
            C1Button btn = sender as C1Button;
            if (int.TryParse(btn.Name[3..], out int num))
                if (resultBox.Value.Equals(StringResource.DefaultResult) || operationPerformed)
                {
                    resultBox.Value = num.ToString();
                    operationPerformed = false;
                }
                else
                    resultBox.Value += num.ToString();
            else
                OperationBtn_Click(btn.Name[3..]);
        }

        // Methods to handle when any operation button is clicked
        private void OperationBtn_Click(string inputOperation)
        {
            // For methods which are not related to the math library
            switch (inputOperation)
            {
                case ".": resultBox.Value += resultBox.Text.Contains('.') ? "" : StringResource.Decimal; return;
                case "Del": resultBox.Value = resultBox.Text.Length > 1 ? resultBox.Text[0..^1] : StringResource.DefaultResult; return;
                case "C": resultBox.Value = operationStack.Value = StringResource.DefaultResult; operator1 = null; operation = null; return;
                case "CE": resultBox.Value = StringResource.DefaultResult; return;
                case "Ans": resultBox.Value = resultBox.Text.Equals(StringResource.DefaultResult) ? result.ToString() : resultBox.Text + result.ToString(); return;
                case "+/-": resultBox.Value = Convert.ToString(Convert.ToDouble(resultBox.Text) * -1); return;
                case "=": if (operation != null) PerformBinaryOp(inputOperation); return;
                default: if (inputOperation != null && inputOperation[0] == 'M') { MemoryBtn_Click(inputOperation); return; } break; // If some random key is pressed from the keyboard in that case the inputOperation will be null and will exit out of the switch case
            }

            // Executes if the operation is related to the math library
            if (operation == null || eval.GetUnaryOperation(inputOperation) != null)
                PerformUnaryOp(inputOperation);
            else
                PerformBinaryOp(inputOperation);
        }

        // Memory options to handle when memory button is clicked
        private void MemoryBtn_Click(string inputOperation)
        {
            switch (inputOperation)
            {
                case "M+": if (btnMR.Items.Count > 0) btnMR.Items[0].Text = (Convert.ToDouble(btnMR.Items[0].Text) + Convert.ToDouble(resultBox.Text)).ToString(); break;
                case "M-": if (btnMR.Items.Count > 0) btnMR.Items[0].Text = (Convert.ToDouble(btnMR.Items[0].Text) - Convert.ToDouble(resultBox.Text)).ToString(); break;
                case "MS": var item = new DropDownItem() { Text = resultBox.Text }; item.Click += MemoryItem_Click; btnMR.Items.Insert(0, item); break;
                case "MR": if (btnMR.Items.Count > 0) MemoryItem_Click(btnMR.Items[0], EventArgs.Empty); break;
                case "MC": btnMR.Items.Clear(); break;
                default: break;
            }
            operationPerformed = true;
        }

        private void MemoryItem_Click(object sender, EventArgs e)
        {
            resultBox.Value = ((DropDownItem)sender).Text;
        }

        // Methods to handle when any menu item button is clicked
        private void MenuItem_Click(Object sender, EventArgs e)
        {
            C1Command menuItem = sender as C1Command;
            switch (menuItem.Name)
            {
                case "Copy": Clipboard.SetText(resultBox.Text); break;
                case "Paste": resultBox.Value = resultBox.Text.Equals(StringResource.DefaultResult) ? Clipboard.GetText() : resultBox.Text + Clipboard.GetText(); break;
                case "Exit": Close(); break;
                case "Help": MessageBox.Show(StringResource.HelpContent, StringResource.HelpTitle); break;
                default: break;
            }
        }

        // To perform unary operation if the input is unary else store the operation to perform binary operation
        private void PerformUnaryOp(string inputOperation)
        {
            if (eval.GetUnaryOperation(inputOperation) != null)
                try
                {
                    result = eval.Evaluate(inputOperation, Convert.ToDouble(resultBox.Text));
                    resultBox.Value = operationStack.Value = result.ToString();
                    if (operation != null) // If operation is not null i.e. user already has some operand and operator in the stack so perform that operation with the current result as operand 2
                        PerformBinaryOp("=");
                }
                catch (ArithmeticException ex)
                {
                    operationStack.Value = ex.Message;
                }
            else
            {
                operation = inputOperation;
                operator1 = Convert.ToDouble(resultBox.Text);
                operationStack.Value = operator1.ToString() + operation;
            }
            operationPerformed = true;
        }

        // To perform binary operation
        private void PerformBinaryOp(string inputOperation)
        {
            try
            {
                result = eval.Evaluate(operation, Convert.ToDouble(operator1), Convert.ToDouble(resultBox.Text));
                resultBox.Value = operationStack.Value = result.ToString();
            }
            catch (ArithmeticException ex)
            {
                operationStack.Value = ex.Message;
            }
            operation = null;
            // If the last pressed operator is '=' calculate the result and set the operator to null otherwise the pressed key is some operation like +, -, etc. so calculate the previous result and add the new operation to the stack
            operator1 = inputOperation != "=" ? result : null;
            operationStack.Value += inputOperation != "=" ? operation = inputOperation : "";
            operationPerformed = true;
        }

        // To handle key event & to restrict certain inputs such as alphabets
        private void KeyInput(Object sender, KeyEventArgs e)
        {
            if (keyBinds.GetString(e.KeyData.ToString()) != null)
                Btn_Click((Button)tableLayoutPanel.Controls[keyBinds.GetString(e.KeyData.ToString())], e);
        }

        [STAThread]
        static void Main()
        {
            Application.Run(new CalcWinForm());
        }
    }
}