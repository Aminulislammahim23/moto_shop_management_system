using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WFADBG;

namespace Chain___Gear.Admin
{
    public partial class ManageEMP : UserControl
    {
        private DataAccess Da { get; set; }

        private string selectedFilePath = string.Empty;
        private string imagePathForDb = string.Empty;
        private string sex = string.Empty;

        public ManageEMP()
        {
            this.Da = new DataAccess();
            InitializeComponent();
            this.PopulateGridView();
            this.dataGridView1.ClearSelection();
            this.AutoIdGenerate();
        }

        private void PopulateGridView(string sql = "SELECT * FROM ManageEMP")
        {
            DataSet ds = this.Da.ExecuteQuery(sql);
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.DataSource = ds.Tables[0];
        }


        private void clearAll()
        {
            this.empIDTxt.Clear();
            this.NameTxt.Clear();
            this.ageTxt.Clear();
            this.nidTxt.Clear();
            this.salaryTxt.Clear();
            this.roleCB.SelectedItem = null;
            this.checkBox1Male.Checked = false;
            this.checkBox2Female.Checked = false;
            this.pbxPhoto.Image = null;
            sex = string.Empty;
            imagePathForDb = string.Empty;
            selectedFilePath = string.Empty;
            this.dataGridView1.ClearSelection();
            this.AutoIdGenerate();
        }

        private void AutoIdGenerate()
        {
            if (roleCB.SelectedItem == null || string.IsNullOrWhiteSpace(roleCB.SelectedItem.ToString()))
                return; // Exit silently if no role selected

            string role = roleCB.SelectedItem.ToString().ToLower();
            string prefix;

            switch (role)
            {
                case "admin":
                    prefix = "a";
                    break;
                case "mechanic":
                    prefix = "m";
                    break;
                case "receptionist":
                    prefix = "r";
                    break;
                default:
                    return; // Exit silently for unknown role
            }

            string sql = $"SELECT EmployeeID FROM ManageEMP WHERE EmployeeID LIKE '{prefix}-%' ORDER BY EmployeeID DESC;";
            DataTable dt = this.Da.ExecuteQueryTable(sql);

            string oldId = dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : $"{prefix}-000";
            string[] temp = oldId.Split('-');
            int n1 = Convert.ToInt32(temp[1]);
            string newId = $"{prefix}-" + (++n1).ToString("D3");

            this.empIDTxt.Text = newId;
        }


        private void saveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Determine gender
                sex = checkBox1Male.Checked ? "Male" : checkBox2Female.Checked ? "Female" : string.Empty;

                if (!this.IsValidToSave())
                {
                    MessageBox.Show("Please fill all the information");
                    return;
                }

                // Check if the employee already exists
                string sql = "SELECT * FROM ManageEMP WHERE EmployeeID = '" + this.empIDTxt.Text + "';";
                DataSet ds = this.Da.ExecuteQuery(sql);

                // Handle image saving
                string destinationFolderPath = Path.Combine(Application.StartupPath, "PDimages");
                Directory.CreateDirectory(destinationFolderPath);

                if (!string.IsNullOrEmpty(selectedFilePath) && selectedFilePath != imagePathForDb)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(selectedFilePath);
                    string destinationFilePath = Path.Combine(destinationFolderPath, fileName);
                    File.Copy(selectedFilePath, destinationFilePath, true);
                    imagePathForDb = destinationFilePath;
                }

                string query1 = ""; // For ManageEMP
                string query2 = ""; // For UserInfo

                if (ds.Tables[0].Rows.Count == 1)
                {
                    // Update existing employee
                    query1 = $"UPDATE ManageEMP SET FullName = '{NameTxt.Text}', Age = '{ageTxt.Text}', NID_No = '{nidTxt.Text}', " +
                             $"Gender = '{sex}', SalaryRange = '{salaryTxt.Text}', AdmitDate = '{dateTimePicker1.Value:yyyy-MM-dd HH:mm:ss}', " +
                             $"Role = '{roleCB.SelectedItem}', Photo = '{imagePathForDb}' WHERE EmployeeID = '{empIDTxt.Text}';";

                    query2 = $"UPDATE UserInfo SET ID = '{empIDTxt.Text}', Name = '{NameTxt.Text}', " +
                             $"Password = '{passwordTxt.Text}', Role = '{roleCB.SelectedItem}' WHERE ID = '{empIDTxt.Text}';";
                }
                else
                {
                    // Insert new employee
                    query1 = $"INSERT INTO ManageEMP (EmployeeID, FullName, Age, NID_No, Gender, SalaryRange, AdmitDate, Role, Photo) VALUES (" +
                             $"'{empIDTxt.Text}', '{NameTxt.Text}', '{ageTxt.Text}', '{nidTxt.Text}', '{sex}', " +
                             $"'{salaryTxt.Text}', '{dateTimePicker1.Value:yyyy-MM-dd HH:mm:ss}', '{roleCB.SelectedItem}', '{imagePathForDb}');";

                    query2 = $"INSERT INTO UserInfo (ID, Name, Password, Role) VALUES (" +
                             $"'{empIDTxt.Text}', '{NameTxt.Text}', '{passwordTxt.Text}', '{roleCB.SelectedItem}');";
                }

                // Execute both queries
                int count1 = this.Da.ExecuteDMLQuery(query1);
                int count2 = this.Da.ExecuteDMLQuery(query2);

                if (count1 == 1 && count2 == 1)
                {
                    MessageBox.Show(ds.Tables[0].Rows.Count == 1 ? "Data Updated Successfully" : "Data Added Successfully");
                }
                else
                {
                    MessageBox.Show("Operation failed, please check!!!");
                }

                this.AutoIdGenerate();
                this.PopulateGridView();
                this.clearAll();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error!! ...please check...\n" + exc.Message);
            }
        }


        private bool IsValidToSave()
        {
            return !string.IsNullOrWhiteSpace(empIDTxt.Text) &&
                   !string.IsNullOrWhiteSpace(NameTxt.Text) &&
                   !string.IsNullOrWhiteSpace(ageTxt.Text) &&
                   !string.IsNullOrWhiteSpace(nidTxt.Text) &&
                   !string.IsNullOrWhiteSpace(sex) &&
                   !string.IsNullOrWhiteSpace(salaryTxt.Text) &&
                   roleCB.SelectedItem != null;
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count < 1)
                {
                    MessageBox.Show("Please select a row first to delete", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                DialogResult result = MessageBox.Show("Are you sure to delete this Item?", "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No) return;

                var empId = this.dataGridView1.CurrentRow.Cells["EmployeeID"].Value.ToString();

                // Delete from both ManageEMP and UserInfo
                string query1 = "DELETE FROM ManageEMP WHERE EmployeeID = '" + empId + "';";
                string query2 = "DELETE FROM UserInfo WHERE ID = '" + empId + "';";

                int count1 = this.Da.ExecuteDMLQuery(query1);
                int count2 = this.Da.ExecuteDMLQuery(query2);

                if (count1 > 0 || count2 > 0)
                {
                    MessageBox.Show("Data removed successfully");
                }
                else
                {
                    MessageBox.Show("Data not removed, please check!!!");
                }

                this.clearAll();
                this.PopulateGridView();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error!! ...please check...\n" + exc.Message);
            }
        }


        private void cancelBtn_Click(object sender, EventArgs e) => this.clearAll();

        private void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                    openFileDialog.Title = "Select a Profile Photo";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedFilePath = openFileDialog.FileName;
                        pbxPhoto.Image = Image.FromFile(selectedFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading photo: " + ex.Message);
            }
        }

        private void searchTxt_TextChanged(object sender, EventArgs e)
        {
            string searchText = this.searchTxt.Text.ToLower();
            string query = "SELECT * FROM ManageEMP WHERE " +
                           $"LOWER(EmployeeID) LIKE '%{searchText}%' OR " +
                           $"LOWER(FullName) LIKE '%{searchText}%' OR " +
                           $"LOWER(Age) LIKE '%{searchText}%' OR " +
                           $"LOWER(NID_No) LIKE '%{searchText}%' OR " +
                           $"LOWER(Gender) LIKE '%{searchText}%' OR " +
                           $"LOWER(SalaryRange) LIKE '%{searchText}%' OR " +
                           $"LOWER(AdmitDate) LIKE '%{searchText}%' OR " +
                           $"LOWER(Role) LIKE '%{searchText}%';";

            DataSet ds = this.Da.ExecuteQuery(query);
            this.dataGridView1.DataSource = ds.Tables[0];
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                this.empIDTxt.Text = this.dataGridView1.CurrentRow.Cells["EmployeeID"].Value.ToString();
                this.NameTxt.Text = this.dataGridView1.CurrentRow.Cells["FullName"].Value.ToString();
                this.ageTxt.Text = this.dataGridView1.CurrentRow.Cells["Age"].Value.ToString();
                this.nidTxt.Text = this.dataGridView1.CurrentRow.Cells["NID_No"].Value.ToString();
                this.salaryTxt.Text = this.dataGridView1.CurrentRow.Cells["SalaryRange"].Value.ToString();
                this.roleCB.SelectedItem = this.dataGridView1.CurrentRow.Cells["Role"].Value.ToString();
                this.dateTimePicker1.Value = Convert.ToDateTime(this.dataGridView1.CurrentRow.Cells["AdmitDate"].Value);
                sex = this.dataGridView1.CurrentRow.Cells["Gender"].Value.ToString();

                checkBox1Male.Checked = sex == "Male";
                checkBox2Female.Checked = sex == "Female";

                imagePathForDb = this.dataGridView1.CurrentRow.Cells["Photo"].Value.ToString();
                if (File.Exists(imagePathForDb))
                    this.pbxPhoto.Image = Image.FromFile(imagePathForDb);
                else
                    this.pbxPhoto.Image = null;
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error loading data: " + exc.Message);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void roleCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            AutoIdGenerate();
        }
    }
}