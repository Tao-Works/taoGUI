using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace taoGUI {
  public partial class Form2 : Form {
    private Form1 _masterForm;

    public Form2(Form1 masterForm) {
      InitializeComponent();
      _masterForm = masterForm;
    }

    private void openFileDialog2_FileOk(object sender, CancelEventArgs e) {

    }

    private void button1_Click(object sender, EventArgs e) {
      bool createTIF = false;

      // Apply block level validation
      if (textBoxCompanyId.TextLength == 0) {
        MessageBox.Show("Company ID is required to generate Tao licence.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      } else if (textBoxProjectId.TextLength == 0) {
        MessageBox.Show("Project ID is required to generate Tao licence.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      } else if (textBoxUserId.TextLength == 0) {
        MessageBox.Show("User ID is required to generate Tao licence.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      } else if (textBoxApplicationName.TextLength == 0) {
        MessageBox.Show("Tao application ID is required.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      } else if (textBoxApplicationFolder.TextLength == 0) {
        MessageBox.Show("Folder name for Tao application '" + textBoxApplicationName.Text + "' is required.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
        // Check if the application ID could be a valid directory name...
        // Check if the folder name could be a valid directory name...
        else {
        // Standard folder structure applies: conf, doc, run, etc...
        string rootPath = textBoxApplicationFolder.Text;
        string applicationPath = rootPath + "\\" + textBoxApplicationName.Text;
        if (!System.IO.Directory.Exists(rootPath)) {
          if (MessageBox.Show("The Tao root folder '" + rootPath + "' does not exist. Do you want to create it?", "New Tao Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
            MessageBox.Show("Creation of Tao application '" + textBoxApplicationName.Text + "' aborted.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          } else {
            applicationPath = textBoxApplicationName.Text;
            createTIF = true;
          }
        } else {
          if (System.IO.Directory.Exists(applicationPath)) {
            MessageBox.Show("Tao folder '" + applicationPath + "' already exists.  Unable to create Tao application.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          } else {
            rootPath = applicationPath;
            applicationPath = "";
            createTIF = true;
          }
        }
        if (createTIF) {
          // Create root folder...
          System.IO.DirectoryInfo rootFolder = new System.IO.DirectoryInfo(rootPath);
          rootFolder.Create();
          if (applicationPath.Length > 0) {
            rootFolder.CreateSubdirectory(applicationPath);
            applicationPath = applicationPath + "\\";
          }
          rootFolder.CreateSubdirectory(applicationPath + "conf");
          // Create Tao conf...
          string taoConfLocation = rootPath + "\\" + applicationPath + @"conf\TaoSuite.conf";
          string applicationNode = textBoxApplicationName.Text.Substring(textBoxApplicationName.Text.LastIndexOf(".") + 1);
          string databaseNode = "Db" + applicationNode.Substring(0, 1).ToUpper() + applicationNode.Substring(1).ToLower();
          using (System.IO.StreamWriter sw = System.IO.File.CreateText(taoConfLocation)) {
            sw.WriteLine("# -- DATA START --");
            sw.WriteLine("[");
            sw.WriteLine("   {");
            sw.WriteLine("      \"taoUserInfo\"         : {");
            sw.WriteLine("         \"companyId\"        : \"" + textBoxCompanyId.Text + "\",");
            sw.WriteLine("         \"projectId\"        : \"" + textBoxProjectId.Text + "\",");
            sw.WriteLine("         \"userId\"           : \"" + textBoxUserId.Text + "\"");
            sw.WriteLine("      },");
            sw.WriteLine("      \"reportDirpath\"       : \"../" + textBoxApplicationName.Text + "/taoSuite_Report/\",");
            sw.WriteLine("      \"stagingDirpath\"      : \"../" + textBoxApplicationName.Text + "/taoSuite_Staging/\",");
            sw.WriteLine("      \"logDirpath\"          : \"../" + textBoxApplicationName.Text + "/taoSuite_Log/\",");
            sw.WriteLine("      \"reportTemplate\"      : \"${filename}.${year}-${month}-${day}_${hour}${min}.${env._taoDbAccess}.${extention}\",");
            sw.WriteLine("      \"shellCall4windows\"   : \"cmd.exe /c ..\\\\" + textBoxApplicationName.Text + "\\\\run\\\\" + applicationNode + ".taoProgramStarter.cmd --action ${runParams} --jdbcDriver '${" + databaseNode + ".jdbcDriver}' --mainDbUrl '${" + databaseNode + ".jdbcUrl}' --resultDbUrl '${" + databaseNode + ".jdbcUrl}'\",");
            sw.WriteLine("      \"shellCall4unix\"      : \"/bin/ksh ../" + textBoxApplicationName.Text + "/run/" + applicationNode + ".taoProgramStarter.sh ${runParams}\",");
            sw.WriteLine("      \"shellTimeoutInMills\" : 10000,");
            sw.WriteLine("      \"userList\"            : [");
            sw.WriteLine("         {");
            sw.WriteLine("            \"userName\"      : \"\",");
            sw.WriteLine("            \"projectName\"   : \"\"");
            sw.WriteLine("         }");
            sw.WriteLine("      ]");
            sw.WriteLine("   }");
            sw.WriteLine("]");
            sw.WriteLine("# -- DATA END --");
            sw.Flush();
            sw.Close();
          }
          // Create SqlScript.conf file...
          string sqlConfLocation = rootPath + "\\" + applicationPath + @"conf\SqlScript.conf";
          string sqlDbFolder = "db" + applicationNode.Substring(0, 1).ToUpper() + applicationNode.Substring(1).ToLower();
          using (System.IO.StreamWriter sw = System.IO.File.CreateText(sqlConfLocation)) {
            sw.WriteLine("# -- DATA START --");
            sw.WriteLine("[");
            sw.WriteLine("   {");
            sw.WriteLine("      \"connectionId\"      : \"" + databaseNode + "\",");
            sw.WriteLine("      \"sqlSetupDirpath\"   : \"../" + textBoxApplicationName.Text + "/sqlSetup/" + sqlDbFolder + "/\",");
            sw.WriteLine("      \"sqlCleanupDirpath\" : \"../" + textBoxApplicationName.Text + "/sqlTeardown/" + sqlDbFolder + "/\"");
            sw.WriteLine("   }");
            sw.WriteLine("]");
            sw.WriteLine("# -- DATA END --");
            sw.Flush();
            sw.Close();
          }
          // Create SqlScript.conf file...
          string issueDate = DateTime.Now.ToString("yyyy-MM-dd");
          string expiryDate = monthCalendarExpiry.SelectionRange.Start.Date.ToString("yyyy-MM-dd");
          string licDataLocation = rootPath + "\\" + applicationPath + @"conf\LicData_" + textBoxCompanyId.Text + "_" + textBoxProjectId.Text + "_" + textBoxUserId.Text + "_" + expiryDate + "_V01-00.txt";
          using (System.IO.StreamWriter sw = System.IO.File.CreateText(licDataLocation)) {
            sw.WriteLine("###########################################");
            sw.WriteLine("# Tao Licence file in plain text");
            sw.WriteLine("#");
            sw.WriteLine("LD_IssueDate=" + issueDate);
            sw.WriteLine("LD_TaoVersion=V01-00");
            sw.WriteLine("LD_TaoProductList=TaoSuite,TaoExcel2DbLoad,TaoExcel2DbCompare,TaoExcelDiffReport");
            sw.WriteLine("LD_EndDate=" + expiryDate);
            sw.WriteLine("LD_CompanyId=" + textBoxCompanyId.Text);
            sw.WriteLine("LD_ProjectId=" + textBoxProjectId.Text);
            sw.WriteLine("LD_UserId=" + textBoxUserId.Text);
            sw.WriteLine("LD_PubKeyRingFile_Digest=");
            sw.Flush();
            sw.Close();
          }
          // Create database connection configuration files...
          string oracleConfLocation = rootPath + "\\" + applicationPath + @"conf\Connection.Oracle.conf";
          using (System.IO.StreamWriter sw = System.IO.File.CreateText(oracleConfLocation)) {
            sw.WriteLine("# -- DATA START --");
            sw.WriteLine("[");
            sw.WriteLine("   {");
            sw.WriteLine("      \"connectionId\" : \"" + databaseNode + "\",");
            sw.WriteLine("      \"jdbcDriver\"   : \"oracle.jdbc.OracleDriver\",");
            sw.WriteLine("      \"username\"     : \"TAO_DB_USER\","); // user must complete this later...
            sw.WriteLine("      \"password\"     : \"TAO_DB_PASS\","); // user must complete this later...
            sw.WriteLine("      \"jdbcUrl\"      : \"jdbc:oracle:thin:@localhost:1521/XE\"");
            sw.WriteLine("   }");
            sw.WriteLine("]");
            sw.WriteLine("# -- DATA END --");
            sw.Flush();
            sw.Close();
          }
          string h2ConfLocation = rootPath + "\\" + applicationPath + @"conf\Connection.H2.conf";
          using (System.IO.StreamWriter sw = System.IO.File.CreateText(h2ConfLocation)) {
            sw.WriteLine("# -- DATA START --");
            sw.WriteLine("[");
            sw.WriteLine("   {");
            sw.WriteLine("      \"connectionId\" : \"" + databaseNode + "\",");
            sw.WriteLine("      \"jdbcDriver\"   : \"org.h2.Driver\",");
            sw.WriteLine("      \"username\"     : \"\",");
            sw.WriteLine("      \"password\"     : \"\",");
            sw.WriteLine("      \"jdbcUrl\"      : \"jdbc:h2:tcp://localhost/" + rootPath.Replace("\\", "/") + "/dba/TaoSuite_" + applicationNode.Substring(0, 1).ToUpper() + applicationNode.Substring(1).ToLower() + ";MODE=Oracle\"");
            sw.WriteLine("   }");
            sw.WriteLine("]");
            sw.WriteLine("# -- DATA END --");
            sw.Flush();
            sw.Close();
          }
          rootFolder.CreateSubdirectory(applicationPath + "dba");
          rootFolder.CreateSubdirectory(applicationPath + "doc");
          rootFolder.CreateSubdirectory(applicationPath + "run");
          string runCmdLocation = rootPath + "\\" + applicationPath + @"run\" + applicationNode + ".taoProgramStarter.cmd";
          using (System.IO.StreamWriter sw = System.IO.File.CreateText(runCmdLocation)) {
            sw.WriteLine("@echo off");
            sw.WriteLine("");
            sw.WriteLine(":: Declare globals variables here...");
            sw.WriteLine("");
            sw.WriteLine(":param_Loop");
            sw.WriteLine("IF [%1]==[] GOTO param_Continue");
            sw.WriteLine("");
            sw.Flush();
            sw.Close();
          }
          rootFolder.CreateSubdirectory(applicationPath + "sandpit");
          rootFolder.CreateSubdirectory(applicationPath + "sqlSetup");
          rootFolder.CreateSubdirectory(applicationPath + "sqlSetup/" + sqlDbFolder);
          rootFolder.CreateSubdirectory(applicationPath + "sqlTeardown");
          rootFolder.CreateSubdirectory(applicationPath + "sqlTeardown/" + sqlDbFolder);
          rootFolder.CreateSubdirectory(applicationPath + "taoSuite_Input");
          rootFolder.CreateSubdirectory(applicationPath + "taoSuite_Log");
          rootFolder.CreateSubdirectory(applicationPath + "taoSuite_Report");
          rootFolder.CreateSubdirectory(applicationPath + "taoSuite_Staging");

          // Create Tao GUI parameter file...
          _masterForm.addProjectFile(textBoxApplicationName.Text, textBoxApplicationDescription.Text, textBoxApplicationFolder.Text);
          // Done!
          MessageBox.Show("Tao application '" + textBoxApplicationName.Text + "' created.", "New Tao Application", MessageBoxButtons.OK, MessageBoxIcon.Information);
          _masterForm.showProjectsInTreeView();
          Close();
        }
      }
    }

    private void button3_Click(object sender, EventArgs e) {
      FolderBrowserDialog searchFolder = new FolderBrowserDialog();
      if (searchFolder.ShowDialog() == DialogResult.OK) {
        textBoxApplicationFolder.Text = searchFolder.SelectedPath;
      }
    }

    private void button4_Click(object sender, EventArgs e) {
      string DriveLetter = System.Environment.GetEnvironmentVariable("SystemDrive");
      openFileDialog2.Filter = "Licence Key (*.lic)|*.lic|Text Document (*.txt)|*.txt|All files (*.*)|*.*";
      openFileDialog2.Title = "Import Tao Licence";
      if (textBoxApplicationFolder.Text.Length > 0) {
        openFileDialog2.InitialDirectory = textBoxApplicationFolder.Text;
      } else {
        openFileDialog2.InitialDirectory = @DriveLetter;
      }
      openFileDialog2.FileName = null;
      openFileDialog2.RestoreDirectory = false;
      if (openFileDialog2.ShowDialog() == DialogResult.OK) {
        // Licence is made up of key and text file...
        string selectedFileName = openFileDialog2.FileName;
        if (selectedFileName.Substring(selectedFileName.LastIndexOf(".") + 1) == "lic") {
          importLicenceKey = selectedFileName;
          importLicenceText = selectedFileName.Substring(0, selectedFileName.LastIndexOf(".") + 1) + "txt";
        } else if (selectedFileName.Substring(selectedFileName.LastIndexOf(".") + 1) == "txt") {
          importLicenceText = selectedFileName;
          importLicenceKey = selectedFileName.Substring(0, selectedFileName.LastIndexOf(".") + 1) + "lic";
        } else {
          importLicenceKey = null;
          importLicenceText = null;
          MessageBox.Show("Not a valid Tao licence file.", "Import Tao Licence", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        // Check the files, load the content, display the results and disable the Tao Licence group...
        if (importLicenceKey != null && importLicenceText != null) {
          System.IO.FileInfo keyInfo = new System.IO.FileInfo(importLicenceKey);
          System.IO.FileInfo textInfo = new System.IO.FileInfo(importLicenceText);
          try {
            if (keyInfo.Length > 0) {
              // Copy licence key to new Tao application area
            }
          } catch {
            importLicenceKey = null;
            MessageBox.Show("Unable to find related Tao licence KEY (encrypted) file.", "Import Tao Licence", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          }
          try {
            if (textInfo.Length > 0) {
              // Read text file, populate form fields and disable (use file data to populate).
            }
          } catch {
            importLicenceText = null;
            if (importLicenceKey.Length > 0) {
              if (MessageBox.Show("Unable to find related Tao licence DESCRIPTION (text) file however, the licence KEY (encrypted) was found.  Do you want to continue?", "Import Tao Licence", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                // Scratch both licence key and text...
                importLicenceKey = null;
              } else {
                // Populate the form fields and diable the text fields (use licence file name to populate).
              }
            } else {
              MessageBox.Show("Unable to find related Tao licence DESCRIPTION (text) file.", "Import Tao Licence", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
          }
        }
        if (importLicenceKey != null) {
          // Get values implied by the licence filename ...
          string licenceFilename = importLicenceKey.Substring(importLicenceKey.LastIndexOf("\\") + 1);
          licenceFilename = licenceFilename.Substring(8, licenceFilename.LastIndexOf("_") - 8);
          textBoxCompanyId.Text = licenceFilename.Substring(0, licenceFilename.IndexOf("_"));
          licenceFilename = licenceFilename.Substring(licenceFilename.IndexOf("_") + 1);
          textBoxProjectId.Text = licenceFilename.Substring(0, licenceFilename.IndexOf("_"));
          licenceFilename = licenceFilename.Substring(licenceFilename.IndexOf("_") + 1);
          textBoxUserId.Text = licenceFilename.Substring(0, licenceFilename.IndexOf("_"));
          licenceFilename = licenceFilename.Substring(licenceFilename.IndexOf("_") + 1);
          //  ... and location of licence file.
          string filePath = importLicenceKey.Substring(0, importLicenceKey.LastIndexOf("\\"));
          filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
          string applicationId = filePath.Substring(filePath.LastIndexOf("\\") + 1);
          applicationId = applicationId.Substring(0, applicationId.LastIndexOf(".") + 1) + "<new application>";
          string rootFolder = filePath.Substring(0, filePath.LastIndexOf("\\"));
          // Default values in form control.
          textBoxApplicationName.Text = applicationId;
          textBoxApplicationDescription.Text = "<application description>";
          textBoxApplicationFolder.Text = rootFolder;
        }
      }
    }

    private void radioButtonTPlus1M_Click(object sender, EventArgs e) {
      System.DateTime licenceEnd = new System.DateTime(System.DateTime.UtcNow.Year,
                                                       System.DateTime.UtcNow.Month,
                                                       System.DateTime.UtcNow.Day);
      licenceEnd = licenceEnd.AddMonths(1);
      monthCalendarExpiry.SetDate(licenceEnd);
    }

    private void radioButtonTPlus3M_Click(object sender, EventArgs e) {
      System.DateTime licenceEnd = new System.DateTime(System.DateTime.UtcNow.Year,
                                                       System.DateTime.UtcNow.Month,
                                                       System.DateTime.UtcNow.Day);
      licenceEnd = licenceEnd.AddMonths(3);
      monthCalendarExpiry.SetDate(licenceEnd);
    }

    private void radioButtonTPlus6M_Click(object sender, EventArgs e) {
      System.DateTime licenceEnd = new System.DateTime(System.DateTime.UtcNow.Year,
                                                       System.DateTime.UtcNow.Month,
                                                       System.DateTime.UtcNow.Day);
      licenceEnd = licenceEnd.AddMonths(6);
      monthCalendarExpiry.SetDate(licenceEnd);
    }

    private void radioButtonTPlus12M_Click(object sender, EventArgs e) {
      System.DateTime licenceEnd = new System.DateTime(System.DateTime.UtcNow.Year,
                                                       System.DateTime.UtcNow.Month,
                                                       System.DateTime.UtcNow.Day);
      licenceEnd = licenceEnd.AddYears(1);
      monthCalendarExpiry.SetDate(licenceEnd);
    }

    private void radioButtonTPlusX_Click(object sender, EventArgs e) {
      System.DateTime licenceEnd = new System.DateTime(System.DateTime.UtcNow.Year, 12, 31);

      if (System.DateTime.UtcNow.Month.Equals(12)) {
        licenceEnd = licenceEnd.AddYears(1);
      }
      monthCalendarExpiry.SetDate(licenceEnd);
    }

    private void monthCalendarExpiry_DateChanged(object sender, EventArgs e) {
      System.DateTime licenceMin = new System.DateTime(System.DateTime.UtcNow.Year,
                                                       System.DateTime.UtcNow.Month,
                                                       System.DateTime.UtcNow.Day);
      licenceMin = licenceMin.AddMonths(1);
      System.DateTime licenceMax = new System.DateTime(System.DateTime.UtcNow.Year, 12, 31);
      if (System.DateTime.UtcNow.Month.Equals(12)) {
        licenceMax = licenceMax.AddYears(1);
      }
      if (System.DateTime.Compare(monthCalendarExpiry.SelectionStart, licenceMin) < 0) {
        if (MessageBox.Show("Minimum licence period for any Tao application is 1 month.", "Licence Agreement", MessageBoxButtons.RetryCancel) == DialogResult.Cancel) {
          Close();
        }
        monthCalendarExpiry.SetDate(licenceMin);
        radioButtonTPlus1M.PerformClick();
      }
      if (System.DateTime.Compare(monthCalendarExpiry.SelectionStart, licenceMax) > 0) {
        if (MessageBox.Show("Maximum licence period exceeded for this Tao application.", "Licence Agreement", MessageBoxButtons.RetryCancel) == DialogResult.Cancel) {
          Close();
        }
        monthCalendarExpiry.SetDate(licenceMax);
        radioButtonTPlusX.PerformClick();
      }
    }
  }
}
