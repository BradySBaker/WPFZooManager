using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Windows;

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
            try {
                string query = "select * from Animal a inner join ZooAnimal za on a.Id = za.AnimalId where za.ZooId = @ZooId";

                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                using (sqlDataAdapter) {
                    sqlCommand.Parameters.AddWithValue("@ZooId", ZooList.SelectedValue);
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


        private void ZooList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            DisplayAssociatedAnimalList();
        }
    }
}
