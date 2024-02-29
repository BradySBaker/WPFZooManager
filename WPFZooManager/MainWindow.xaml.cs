using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace WPFZooManager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        SqlConnection sqlConnection;
        public MainWindow() {
            InitializeComponent();

            string connectionString = ConfigurationManager.ConnectionStrings["WPFZooManager.Properties.Settings.ZooDBConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connectionString);
            DisplayZooList();
            DisplayAnimalList();
        }

        private void DisplayZooList() {
            try {
                string query = "select * from Zoo";
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlDataAdapter) {
                    DataTable zooTable = new DataTable();

                    sqlDataAdapter.Fill(zooTable);

                    ZooList.DisplayMemberPath = "Location";
                    ZooList.SelectedValuePath = "Id";
                    ZooList.ItemsSource = zooTable.DefaultView;
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }
        private void DisplayAssociatedAnimalList() {
            object zooId = ZooList.SelectedValue;
            if (zooId == null) {
                return;
            }
            try {
                string query = "select * from Animal a inner join ZooAnimal za on a.Id = za.AnimalId where za.ZooId = " + zooId;

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter) {
                    DataTable animalTable = new DataTable();

                    sqlDataAdapter.Fill(animalTable);

                    AssociatedAnimalsList.DisplayMemberPath = "Name";
                    AssociatedAnimalsList.SelectedValuePath = "Id";
                    AssociatedAnimalsList.ItemsSource = animalTable.DefaultView;
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private void DisplayAnimalList() {
            try {
                string query = "select * from Animal";

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                using (sqlDataAdapter) {
                    DataTable animalTable = new DataTable();

                    sqlDataAdapter.Fill(animalTable);

                    AnimalList.DisplayMemberPath = "Name";
                    AnimalList.SelectedValuePath = "Id";
                    AnimalList.ItemsSource = animalTable.DefaultView;
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }


        private void ZooList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            object zooId = ZooList.SelectedValue;
            if (zooId == null) {
                return;
            }
            NameInput.Text = ((DataRowView)ZooList.SelectedItem)["Location"].ToString();
            DisplayAssociatedAnimalList();
        }


        private async void AddAnimalToZoo_Clicked(object sender, RoutedEventArgs ev) {
            object zooId = ZooList.SelectedValue;
            object animalId = AnimalList.SelectedValue;
            if (zooId == null || animalId == null) {
                MessageBox.Show("Please select a zoo and a animal first");
                return;
            }
            string insertQuery = $"INSERT INTO ZooAnimal (ZooId, AnimalId) VALUES ({zooId}, {animalId})";

            SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection);

            try {
                sqlConnection.Open();
                await insertCommand.ExecuteNonQueryAsync();
                DisplayAssociatedAnimalList();
                sqlConnection.Close();
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private async void DeleteZoo_Clicked(object sender, RoutedEventArgs e) {
            object zooId = ZooList.SelectedValue;
            if (zooId == null) {
                MessageBox.Show("Please select a Zoo");
                return;
            }
            await UpdateTable("Delete from Zoo Where Id = " + zooId);
            DisplayZooList();
            AssociatedAnimalsList.ItemsSource = null;
        }


        private async void DeleteAnimal_Clicked(object sender, RoutedEventArgs e) {
            object animalId = AnimalList.SelectedValue;
            if (animalId == null) {
                MessageBox.Show("Please select a Animal");
                return;
            }
            await UpdateTable("Delete from Animal Where Id = " + animalId);
            DisplayAnimalList();

        }


        private async void RemoveAnimal_Clicked(object sender, RoutedEventArgs e) { //Removes animal from zoo
            object animalId = AssociatedAnimalsList.SelectedValue;
            if (animalId == null) {
                MessageBox.Show("Please select a Animal");
                return;
            }
            string deleteQuery = "Delete from ZooAnimal Where AnimalId = " + animalId;
            await UpdateTable(deleteQuery);
            DisplayAssociatedAnimalList();
        }


        private async void AddZoo_Clicked(object sender, RoutedEventArgs e) {
            string zooLocation = NameInput.Text;
            if (!string.IsNullOrEmpty(zooLocation)) {
                string addQuery = $"Insert into Zoo (Location) Values ('{zooLocation}')";
                await UpdateTable(addQuery);
                DisplayZooList();
            } else {
                MessageBox.Show("Please input a zoo location");
            }
        }

        private async void AddAnimal_Clicked(object sender, RoutedEventArgs e) {
            string animalName = NameInput.Text;
            if (!string.IsNullOrEmpty(animalName)) {
                string addQuery = $"Insert into Animal (Name) Values ('{animalName}')";
                await UpdateTable(addQuery);
                DisplayAnimalList();
            } else {
                MessageBox.Show("Please input a animal name");
            }
        }

        private async void UpdateZoo_Clicked(object sender, RoutedEventArgs e) {
            string zooLocation = NameInput.Text;
            object zooId = ZooList.SelectedValue;

            if (!string.IsNullOrEmpty(zooLocation) && zooId != null) {
                string updateQuery = $"Update Zoo Set Location = '{zooLocation}' Where Id = {zooId}";
                await UpdateTable(updateQuery);
                DisplayZooList();
            } else {
                MessageBox.Show("Please input a zoo location and choose a zoo");
            }
        }

        private async void UpdateAnimal_Clicked(object sender, RoutedEventArgs e) {
            object newAnimalId = AnimalList.SelectedValue;
            object curAnimalId = AssociatedAnimalsList.SelectedValue;
            object zooId = ZooList.SelectedValue;
            if (newAnimalId != null && curAnimalId != null && zooId != null) {
               string updateQuery = $"Update ZooAnimal Set AnimalId = '{newAnimalId}' Where ZooId={zooId} And AnimalId={curAnimalId}";
                await UpdateTable(updateQuery);
                DisplayAssociatedAnimalList();
            } else {
                MessageBox.Show("Please choose a zoo, a associated animal, and a animal from the animal list");
            }
        }


        private async Task UpdateTable(string query) {
            try {
                SqlCommand command = new SqlCommand(query, sqlConnection);

                sqlConnection.Open();
                await command.ExecuteNonQueryAsync();
                sqlConnection.Close();
            } catch (Exception ex) {
                MessageBox.Show("Error updating table: " + ex.Message);
                sqlConnection.Close();
            }
        }

    }
}
