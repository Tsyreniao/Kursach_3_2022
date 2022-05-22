using System;
using System.Data;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace Kursach_3
{
    public partial class Form1 : Form
    {
        private int selectedRowId = -1;
        private bool isSorted = false;

        private SqlConnection sqlConnection = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["StudentsDB"].ConnectionString);
            sqlConnection.Open();

            if (sqlConnection.State != ConnectionState.Open)
            {
                MessageBox.Show("Connection isnt open!");
            }

            Cleaner();
        }

        private void Cleaner()
        {
            ClearTextBoxes();
            ShowDB();
            dataGridView1.ClearSelection();
            selectedRowId = -1;
            isSorted = false;
        }

        private void AddStudentBtn_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 || textBox2.Text.Length > 0 || textBox3.Text.Length > 0 || textBox4.Text.Length > 0)
            {
                SqlCommand command = new SqlCommand(
                "Insert Into [StudentsInfo] ([Name], Surname, [Group], Email) Values (@Name, @Surname, @Group, @Email)",
                sqlConnection);

                command.Parameters.AddWithValue("Name", textBox1.Text);
                command.Parameters.AddWithValue("Surname", textBox2.Text);
                command.Parameters.AddWithValue("Group", textBox3.Text);
                command.Parameters.AddWithValue("Email", textBox4.Text);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                Cleaner();
            }
            else MessageBox.Show("Enter at least one parameter");
        }

        private void ClearTextBoxes()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
        }

        private void ShowDB()
        {
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(
                "Select * From [StudentsInfo]",
                sqlConnection);

            DataSet dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);

            dataGridView1.DataSource = dataSet.Tables[0];
            dataGridView1.AllowUserToAddRows = false;
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (selectedRowId != -1)
            {
                SqlCommand command = new SqlCommand(
                    "Delete From [StudentsInfo] Where Id=@Id",
                    sqlConnection);

                command.Parameters.AddWithValue("Id", selectedRowId);
                command.ExecuteNonQuery();

                Cleaner();
            }
            else MessageBox.Show("Select sell or row");
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedRowId = Convert.ToInt32(row.Cells["Id"].Value.ToString());

                textBox1.Text = row.Cells["Name"].Value.ToString();
                textBox2.Text = row.Cells["Surname"].Value.ToString();
                textBox3.Text = row.Cells["Group"].Value.ToString();
                textBox4.Text = row.Cells["Email"].Value.ToString();
            }
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                selectedRowId = Convert.ToInt32(row.Cells["Id"].Value.ToString());

                textBox1.Text = row.Cells["Name"].Value.ToString();
                textBox2.Text = row.Cells["Surname"].Value.ToString();
                textBox3.Text = row.Cells["Group"].Value.ToString();
                textBox4.Text = row.Cells["Email"].Value.ToString();
            }
        }

        private void UpdateDataBtn_Click(object sender, EventArgs e)
        {
            if (selectedRowId != -1)
            {
                SqlCommand command = new SqlCommand(
                    "Update [StudentsInfo] Set Name=@Name, Surname=@Surname, [Group]=@Group, Email=@Email Where Id=@Id",
                    sqlConnection);

                command.Parameters.AddWithValue("Id", selectedRowId);
                command.Parameters.AddWithValue("Name", textBox1.Text);
                command.Parameters.AddWithValue("Surname", textBox2.Text);
                command.Parameters.AddWithValue("Group", textBox3.Text);
                command.Parameters.AddWithValue("Email", textBox4.Text);
                command.ExecuteNonQuery();

                Cleaner();
            }
            else MessageBox.Show("Select sell or row");
        }

        private void ExportDbAsXmls_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        if (isSorted == false)
                        {
                            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(
                            "Select * From [StudentsInfo]",
                            sqlConnection);

                            DataTable dataTable = new DataTable();
                            sqlDataAdapter.Fill(dataTable);

                            workbook.Worksheets.Add(dataTable, "StudentsInfo");
                            workbook.SaveAs(sfd.FileName);
                        }
                        else
                        {
                            string parameters = TextboxesString();

                            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(
                            $"Select * From [StudentsInfo] Where {parameters}",
                            sqlConnection);

                            DataTable dataTable = new DataTable();
                            sqlDataAdapter.Fill(dataTable);

                            workbook.Worksheets.Add(dataTable, "StudentsInfo");
                            workbook.SaveAs(sfd.FileName);
                        }
                    }
                    MessageBox.Show("DataBase is successful exported as excel file");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            string parameters = TextboxesString();

            if (parameters.Length > 1)
            {
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(
                $"Select * From [StudentsInfo] Where {parameters}",
                sqlConnection);

                DataSet dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);

                dataGridView1.DataSource = dataSet.Tables[0];
                dataGridView1.AllowUserToAddRows = false;

                isSorted = true;
            }
            else
            {
                MessageBox.Show("Enter parameters");
            }
            dataGridView1.ClearSelection();
            selectedRowId = -1;
        }

        private string TextboxesString()
        {
            string parameters = "";
            string parametersMinus1 = "";

            if (textBox1.Text != "")
            {
                parameters += "Name Like ";
                parameters += "N'%";
                parameters += textBox1.Text;
                parameters += "%'";
                parameters += "And ";
            }
            if (textBox2.Text != "")
            {
                parameters += "Surname Like ";
                parameters += "N'%";
                parameters += textBox2.Text;
                parameters += "%'";
                parameters += "And ";
            }
            if (textBox3.Text != "")
            {
                parameters += "[Group] Like ";
                parameters += "N'%";
                parameters += textBox3.Text;
                parameters += "%'";
                parameters += "And ";
            }
            if (textBox4.Text != "")
            {
                parameters += "Email Like ";
                parameters += "N'%";
                parameters += textBox4.Text;
                parameters += "%'";
                parameters += "And ";
            }
            if (parameters.Length > 0)
            {
                parametersMinus1 = parameters.Remove(parameters.Length - 4, 4);
            }
            return parametersMinus1;
        }

        private void ShowDbBtn_Click(object sender, EventArgs e)
        {
            Cleaner();
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}