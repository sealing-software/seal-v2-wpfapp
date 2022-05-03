using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using SEAL_V2.model;
using System.Security.Cryptography;
using SEAL_V2.view.usercontrolobjects;
using System.Management;
using Microsoft.Win32;
using regex = System.Text.RegularExpressions;

namespace SEAL_V2.db
{
    class DatabaseInterface
    {
        private static DatabaseInterface instance = null;
        private static readonly object padlock = new object();
        private String databaseLocation = @"URI=file:" + Path.Combine(Environment.CurrentDirectory, @"db/SEAL.db");
        private SQLiteConnection connection;
        public event EventHandler<StatusMessage> dbStatus;

        private DatabaseInterface()
        {
            connect();

            getConnectionState();

            initialSetup();
        }

        public static DatabaseInterface Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DatabaseInterface();
                    }
                    return instance;
                }
            }
        }

        private void connect()
        {
            connectToDb(databaseLocation);
            sendConnectionData();
        }

        private void connectToDb(String dbLocation)
        {
            connection = new SQLiteConnection(dbLocation);
            connection.Open();
        }

        private void disconnect()
        {
            connection.Close();
            sendConnectionData();
        }

        private System.Data.ConnectionState getConnectionState()
        {
            return connection.State;
            //Console.WriteLine(connection.State.GetType());
            //if (getConnectionState == System.Data.ConnectionState.Broken)
        }

        //Used to relay status message to other modules 1 - DB, 2 - db status... MESSAGING NEEDS TO BE FIXED
        private void sendConnectionData()
        {
            if (this.dbStatus != null)
            {
                StatusMessage message = new StatusMessage(1, getConnectionState(), 1);

                this.dbStatus(this, message);
            }
        }

        public void requestdbStatus()
        {
            sendConnectionData();
        }

        private void initialSetup()
        {
            initialAdminAccountSetup();

            setupSequencesTable();
        }

        private void initialAdminAccountSetup()
        {
            groupSetup();
        }

        private void groupSetup()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS groups(id INTEGER PRIMARY KEY, name TEXT, hexcolor TEXT)";
            cmd.ExecuteNonQuery();
            if (!dupeGroupExists("admin"))
            {
                cmd.CommandText = "INSERT INTO groups(name, hexcolor) VALUES('admin', '#FFFFFF')";
                cmd.ExecuteNonQuery();
                defaultAdminAccountSetup();
                defaultAdminSettingsSetup();
            }
        }

        public bool dupeGroupExists(String groupName)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupNameParam = new SQLiteParameter();
            groupNameParam.ParameterName = "@groupname";
            groupNameParam.Value = groupName;

            cmd.Parameters.Add(groupNameParam);


            cmd.CommandText = "SELECT COUNT(1) FROM groups WHERE name = @groupname";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }




        public void addGroup(String groupName, String groupColorHex)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupNameParam = new SQLiteParameter();
            groupNameParam.ParameterName = "@groupname";
            groupNameParam.Value = groupName;

            cmd.Parameters.Add(groupNameParam);

            SQLiteParameter hexColorParam = new SQLiteParameter();
            hexColorParam.ParameterName = "@hexcolor";
            hexColorParam.Value = groupColorHex;

            cmd.Parameters.Add(hexColorParam);

            cmd.CommandText = "INSERT INTO groups(name, hexcolor) VALUES(@groupname, @hexcolor)";

            cmd.ExecuteNonQuery();
        }

        public void addUser(String username, String displayName, String password, Group assignedGroup)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter usernameParam = new SQLiteParameter();
            usernameParam.ParameterName = "@username";
            usernameParam.Value = username;

            cmd.Parameters.Add(usernameParam);

            SQLiteParameter displayNameParam = new SQLiteParameter();
            displayNameParam.ParameterName = "@displayname";
            displayNameParam.Value = displayName;

            cmd.Parameters.Add(displayNameParam);

            SQLiteParameter passwordParam = new SQLiteParameter();
            passwordParam.ParameterName = "@password";
            using (var sha256 = new SHA256Managed())
            {
                passwordParam.Value = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
            }

            cmd.Parameters.Add(passwordParam);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@assignedgroup";
            groupParam.Value = assignedGroup.ID;

            cmd.Parameters.Add(groupParam);

            cmd.CommandText = "INSERT INTO accounts(username, password, name, groupid) VALUES(@username, @password, @displayname, @assignedgroup)";

            cmd.ExecuteNonQuery();
        }


        private void defaultAdminAccountSetup()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS accounts(id INTEGER PRIMARY KEY, username TEXT, password TEXT, name TEXT, groupid INTEGER, FOREIGN KEY (groupid) REFERENCES groups(id))";
            cmd.ExecuteNonQuery();

            SQLiteParameter passwordParam = new SQLiteParameter();
            passwordParam.ParameterName = "@password";
            using (var sha256 = new SHA256Managed())
            {
                passwordParam.Value = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("admin"))).Replace("-", "");
            }

            cmd.Parameters.Add(passwordParam);

            cmd.CommandText = "INSERT INTO accounts(username, password, name, groupid) VALUES('admin', @password, 'Admin', 1)";
            cmd.ExecuteNonQuery();
        }

        private void defaultAdminSettingsSetup()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS grouppermissions(id INTEGER PRIMARY KEY, groupid INTEGER, objectname TEXT, FOREIGN KEY (groupid) REFERENCES groups(id))";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO grouppermissions(groupid, objectname) VALUES(1, 'Settings_Page_List_Users')";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO grouppermissions(groupid, objectname) VALUES(1, 'Settings_Page_List_Groups')";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO grouppermissions(groupid, objectname) VALUES(1, 'Settings_Page_List_Data_Structure')";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO grouppermissions(groupid, objectname) VALUES(1, 'Settings_Page_List_Themes')";
            cmd.ExecuteNonQuery();


            ///TEST THIS NEEDS TO BE REMOVED!!!!!
            cmd.CommandText = "INSERT INTO grouppermissions(groupid, objectname) VALUES(2, 'Settings_Page_List_Themes')";
            cmd.ExecuteNonQuery();
        }

        //Account check, return true if username AND password is found.... password SHOULD be hashed... FUTURE WORK
        public bool checkAccountExists(String userName, String password)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter usernameParam = new SQLiteParameter();
            usernameParam.ParameterName = "@username";
            usernameParam.Value = userName;

            cmd.Parameters.Add(usernameParam);

            SQLiteParameter passwordParam = new SQLiteParameter();
            passwordParam.ParameterName = "@password";
            using (var sha256 = new SHA256Managed())
            {
                passwordParam.Value = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
            }

            cmd.Parameters.Add(passwordParam);

            cmd.CommandText = "SELECT COUNT(1) FROM accounts WHERE username = @username AND password = @password";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;

                cmd.CommandText = "SELECT * FROM accounts WHERE username = @username AND password = @password";

                SQLiteDataReader read = cmd.ExecuteReader();

                while (read.Read())
                {
                    User.userInfo(read["username"].ToString(), read["name"].ToString(), Int32.Parse(read["groupid"].ToString()), Int32.Parse(read["id"].ToString()));
                }

            }

            return found;
        }

        public bool checkGroupNameExists(String groupName)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupname";
            groupParam.Value = groupName;

            cmd.Parameters.Add(groupParam);

            cmd.CommandText = "SELECT COUNT(1) FROM groups WHERE LOWER(name) LIKE LOWER(@groupname)";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool checkUsernameExists(String username)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter usernameParam = new SQLiteParameter();
            usernameParam.ParameterName = "@username";
            usernameParam.Value = username;

            cmd.Parameters.Add(usernameParam);

            cmd.CommandText = "SELECT COUNT(1) FROM accounts WHERE LOWER(username) LIKE LOWER(@username)";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public String getAccountGroupText(int groupID)
        {
            String groupName = "";

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupIDParam = new SQLiteParameter();
            groupIDParam.ParameterName = "@groupID";
            groupIDParam.Value = groupID;

            cmd.Parameters.Add(groupIDParam);

            cmd.CommandText = "SELECT * FROM groups WHERE id = @groupID";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                groupName = read["name"].ToString();
            }

            return groupName;
        }

        public String getGroupHexColor(int userGroupID)
        {
            String hexColor = "";

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupIDParam = new SQLiteParameter();
            groupIDParam.ParameterName = "@groupID";
            groupIDParam.Value = userGroupID;

            cmd.Parameters.Add(groupIDParam);

            cmd.CommandText = "SELECT * FROM groups WHERE id = @groupID";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                hexColor = read["hexcolor"].ToString();
            }

            return hexColor;
        }

        public bool checkGroupObjectPermission(int group, String objName)
        {
            bool authorized = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupid";
            groupParam.Value = group;

            cmd.Parameters.Add(groupParam);

            SQLiteParameter objNameParam = new SQLiteParameter();
            objNameParam.ParameterName = "@objname";
            objNameParam.Value = objName;

            cmd.Parameters.Add(objNameParam);

            cmd.CommandText = "SELECT COUNT(1) FROM grouppermissions WHERE groupid = @groupid AND objectname = @objname";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                authorized = true;
            }

            return authorized;
        }

        public void addPermission(int group, String objName)
        {
            if (!checkGroupObjectPermission(group, objName))
            {
                using var cmd = new SQLiteCommand(connection);

                SQLiteParameter groupParam = new SQLiteParameter();
                groupParam.ParameterName = "@group";
                groupParam.Value = group;

                cmd.Parameters.Add(groupParam);

                SQLiteParameter nameParam = new SQLiteParameter();
                nameParam.ParameterName = "@objName";
                nameParam.Value = objName;

                cmd.Parameters.Add(nameParam);

                cmd.CommandText = "INSERT INTO grouppermissions(groupid, objectname) VALUES(@group, @objname)";

                cmd.ExecuteNonQuery();
            }
        }

        public void removePermission(int group, String objName)
        {
            if (checkGroupObjectPermission(group, objName))
            {
                using var cmd = new SQLiteCommand(connection);

                SQLiteParameter groupParam = new SQLiteParameter();
                groupParam.ParameterName = "@group";
                groupParam.Value = group;

                cmd.Parameters.Add(groupParam);

                SQLiteParameter nameParam = new SQLiteParameter();
                nameParam.ParameterName = "@objName";
                nameParam.Value = objName;

                cmd.Parameters.Add(nameParam);

                cmd.CommandText = "DELETE FROM grouppermissions WHERE groupid = @group AND objectname = @objname";

                cmd.ExecuteNonQuery();
            }
        }

        public List<Group> getGroups()
        {
            List<Group> groupList = new List<Group>();

            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "SELECT * FROM groups";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                groupList.Add(new Group(Int32.Parse(read["id"].ToString()), read["name"].ToString(), read["hexcolor"].ToString()));
            }


            foreach (Group group in groupList)
            {
                using var cmd1 = new SQLiteCommand(connection);

                SQLiteParameter groupParam = new SQLiteParameter();
                groupParam.ParameterName = "@groupid";
                groupParam.Value = group.ID;

                cmd1.Parameters.Add(groupParam);

                cmd1.CommandText = "SELECT COUNT(*) FROM accounts WHERE groupid = @groupid";

                var result = cmd1.ExecuteScalar();
                group.qty = Convert.ToInt32(result);
            }

            return groupList;
        }

        public Group getGroup(int groupID)
        {
            Group foundGroup = null;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupid";
            groupParam.Value = groupID;

            cmd.Parameters.Add(groupParam);

            cmd.CommandText = "SELECT * FROM groups WHERE id = @groupid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                foundGroup = new Group(Int32.Parse(read["id"].ToString()), read["name"].ToString(), read["hexcolor"].ToString());
            }

            return foundGroup;
        }

        public List<UserInfo> getUsersFromGroup(int groupID)
        {
            List<UserInfo> tempList = new List<UserInfo>();


            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupid";
            groupParam.Value = groupID;


            cmd.Parameters.Add(groupParam);

            cmd.CommandText = "SELECT * FROM accounts WHERE groupid = @groupid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                tempList.Add(new UserInfo(Int32.Parse(read["id"].ToString()), read["username"].ToString(), read["name"].ToString(), Int32.Parse(read["groupid"].ToString())));
            }

            return tempList;
        }

        public List<UserInfo> getAllUsers()
        {
            List<UserInfo> tempList = new List<UserInfo>();


            using var cmd = new SQLiteCommand(connection);


            cmd.CommandText = "SELECT * FROM accounts";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                tempList.Add(new UserInfo(Int32.Parse(read["id"].ToString()), read["username"].ToString(), read["name"].ToString(), Int32.Parse(read["groupid"].ToString())));
            }

            return tempList;
        }

        public void updateGroupColor(int groupID, String hexColor)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupid";
            groupParam.Value = groupID;

            cmd.Parameters.Add(groupParam);

            SQLiteParameter hexColorParam = new SQLiteParameter();
            hexColorParam.ParameterName = "@hexcolor";
            hexColorParam.Value = hexColor;

            cmd.Parameters.Add(hexColorParam);

            cmd.CommandText = "UPDATE groups SET hexcolor = @hexcolor WHERE id = @groupid";

            cmd.ExecuteNonQuery();
        }

        public void updateGroupName(int groupID, String newName)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupid";
            groupParam.Value = groupID;

            cmd.Parameters.Add(groupParam);

            SQLiteParameter nameParam = new SQLiteParameter();
            nameParam.ParameterName = "@newname";
            nameParam.Value = newName;

            cmd.Parameters.Add(nameParam);

            cmd.CommandText = "UPDATE groups SET name = @newname WHERE id = @groupid";

            cmd.ExecuteNonQuery();
        }

        public void deleteGroup(Group passedGroup)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupname";
            groupParam.Value = passedGroup.name;

            cmd.Parameters.Add(groupParam);

            cmd.CommandText = "DELETE FROM groups WHERE name = @groupname";

            cmd.ExecuteNonQuery();
        }

        public void updateUser(int userID, String name, String password, Group selectedGroup)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter userParam = new SQLiteParameter();
            userParam.ParameterName = "@userid";
            userParam.Value = userID;

            cmd.Parameters.Add(userParam);

            SQLiteParameter userNameParam = new SQLiteParameter();
            userNameParam.ParameterName = "@name";
            userNameParam.Value = name;

            cmd.Parameters.Add(userNameParam);

            SQLiteParameter passwordParam = new SQLiteParameter();
            passwordParam.ParameterName = "@password";
            using (var sha256 = new SHA256Managed())
            {
                passwordParam.Value = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
            }
            cmd.Parameters.Add(passwordParam);


            SQLiteParameter groupParam = new SQLiteParameter();
            groupParam.ParameterName = "@groupid";
            groupParam.Value = selectedGroup.ID;

            cmd.Parameters.Add(groupParam);

            if (password.Equals(""))
            {
                cmd.CommandText = "UPDATE accounts SET name = @name, groupid = @groupid WHERE id = @userid";
            }
            else
            {
                cmd.CommandText = "UPDATE accounts SET name = @name, password = @password, groupid = @groupid WHERE id = @userid";
            }

            cmd.ExecuteNonQuery();
        }

        public void updateUserPassword(int userID, String password)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter userParam = new SQLiteParameter();
            userParam.ParameterName = "@userid";
            userParam.Value = userID;

            cmd.Parameters.Add(userParam);


            SQLiteParameter passwordParam = new SQLiteParameter();
            passwordParam.ParameterName = "@password";
            using (var sha256 = new SHA256Managed())

            {
                passwordParam.Value = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
            }
            cmd.Parameters.Add(passwordParam);

            cmd.CommandText = "UPDATE accounts SET password = @password WHERE id = @userid";

            cmd.ExecuteNonQuery();
        }

        public void deleteUser(UserInfo passedUser)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter userParam = new SQLiteParameter();
            userParam.ParameterName = "@userid";
            userParam.Value = passedUser.ID;

            cmd.Parameters.Add(userParam);

            cmd.CommandText = "DELETE FROM accounts WHERE id = @userid";

            cmd.ExecuteNonQuery();
        }

        private void setupSequencesTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS sequences(id INTEGER PRIMARY KEY, name TEXT, description TEXT, sequence_string TEXT, systemlock INTEGER)";
            cmd.ExecuteNonQuery();
        }

        public bool checkSequenceNameExists(String sequence_name)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter sequenceParam = new SQLiteParameter();
            sequenceParam.ParameterName = "@sequencename";
            sequenceParam.Value = sequence_name;

            cmd.Parameters.Add(sequenceParam);

            cmd.CommandText = "SELECT COUNT(1) FROM sequences WHERE LOWER(name) LIKE LOWER(@sequencename)";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public void saveSequence(String name, String description, String sequence, int systemLock)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter sequenceNameParam = new SQLiteParameter();
            sequenceNameParam.ParameterName = "@sequencename";
            sequenceNameParam.Value = name;

            cmd.Parameters.Add(sequenceNameParam);

            SQLiteParameter sequenceParam = new SQLiteParameter();
            sequenceParam.ParameterName = "@sequence";
            sequenceParam.Value = sequence;

            cmd.Parameters.Add(sequenceParam);

            SQLiteParameter descriptionParam = new SQLiteParameter();
            descriptionParam.ParameterName = "@description";
            descriptionParam.Value = description;

            cmd.Parameters.Add(descriptionParam);

            SQLiteParameter systemLockParam = new SQLiteParameter();
            systemLockParam.ParameterName = "@systemlock";
            systemLockParam.Value = systemLock;

            cmd.Parameters.Add(systemLockParam);


            cmd.CommandText = "INSERT INTO sequences(name, description, sequence_string, systemlock) VALUES(@sequencename, @description, @sequence, @systemlock)";

            cmd.ExecuteNonQuery();
        }

        public List<Sequence> getSequences()
        {
            List<Sequence> sequenceList = new List<Sequence>();

            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "SELECT * FROM sequences";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                sequenceList.Add(new Sequence(Int32.Parse(read["id"].ToString()), read["name"].ToString(), read["description"].ToString(), read["sequence_string"].ToString(), Int32.Parse(read["systemlock"].ToString())));
            }

            return sequenceList;
        }

        public Sequence getSequence(int sequenceID)
        {
            Sequence found = null;
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter sequenceIDParam = new SQLiteParameter();
            sequenceIDParam.ParameterName = "@sequenceid";
            sequenceIDParam.Value = sequenceID;

            cmd.Parameters.Add(sequenceIDParam);

            cmd.CommandText = "SELECT * FROM sequences WHERE id = @sequenceid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                found = new Sequence(Int32.Parse(read["id"].ToString()), read["name"].ToString(), read["description"].ToString(), read["sequence_string"].ToString(), Int32.Parse(read["systemlock"].ToString()));
            }

            return found;
        }

        public void editSequence(int sequenceID, String name, String description, String sequence, int systemLock)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter sequenceIDParam = new SQLiteParameter();
            sequenceIDParam.ParameterName = "@sequenceid";
            sequenceIDParam.Value = sequenceID;

            cmd.Parameters.Add(sequenceIDParam);

            SQLiteParameter sequenceNameParam = new SQLiteParameter();
            sequenceNameParam.ParameterName = "@sequencename";
            sequenceNameParam.Value = name;

            cmd.Parameters.Add(sequenceNameParam);

            SQLiteParameter sequenceParam = new SQLiteParameter();
            sequenceParam.ParameterName = "@sequence";
            sequenceParam.Value = sequence;

            cmd.Parameters.Add(sequenceParam);

            SQLiteParameter descriptionParam = new SQLiteParameter();
            descriptionParam.ParameterName = "@description";
            descriptionParam.Value = description;

            cmd.Parameters.Add(descriptionParam);

            SQLiteParameter systemLockParam = new SQLiteParameter();
            systemLockParam.ParameterName = "@systemlock";
            systemLockParam.Value = systemLock;

            cmd.Parameters.Add(systemLockParam);

            cmd.CommandText = "UPDATE sequences SET name = @sequencename, description = @description, sequence_string = @sequence, systemlock = @systemlock WHERE id = @sequenceid";

            cmd.ExecuteNonQuery();
        }

        public void deleteSequence(Sequence passedSeqeunce)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter sequenceIDParam = new SQLiteParameter();
            sequenceIDParam.ParameterName = "@sequenceid";
            sequenceIDParam.Value = passedSeqeunce.getID();

            cmd.Parameters.Add(sequenceIDParam);

            cmd.CommandText = "DELETE FROM sequences WHERE id = @sequenceid";

            cmd.ExecuteNonQuery();
        }

        public void createDirectoriesTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS directories(id INTEGER PRIMARY KEY, name TEXT, parentid INTEGER, sequenceapplied INT, sequenceid INT)";
            cmd.ExecuteNonQuery();
        }

        public void addDirectory(String name, int parentID)
        {
            createDirectoriesTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter directoryNameParam = new SQLiteParameter();
            directoryNameParam.ParameterName = "@directoryname";
            directoryNameParam.Value = name;

            cmd.Parameters.Add(directoryNameParam);

            SQLiteParameter parentIDParam = new SQLiteParameter();
            parentIDParam.ParameterName = "@parentid";
            parentIDParam.Value = parentID;

            cmd.Parameters.Add(parentIDParam);

            cmd.CommandText = "INSERT INTO directories(name, parentid, sequenceapplied, sequenceid) VALUES(@directoryname, @parentid, 0, 0)";

            cmd.ExecuteNonQuery();
        }

        public bool checksubDirectoryExists(String directoryName, DirectoryItem parentDirectory)
        {
            bool found = false;

            int parentID = 0;

            if (parentDirectory != null)
            {
                parentID = parentDirectory.id;
            }

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter directNameParam = new SQLiteParameter();
            directNameParam.ParameterName = "@directname";
            directNameParam.Value = directoryName;

            cmd.Parameters.Add(directNameParam);

            SQLiteParameter directIDParam = new SQLiteParameter();
            directIDParam.ParameterName = "@directid";
            directIDParam.Value = parentID;

            cmd.Parameters.Add(directIDParam);

            cmd.CommandText = "SELECT COUNT(1) FROM directories WHERE LOWER(name) LIKE LOWER(@directname) AND parentid = @directid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool checkSameLevelDirectoryExists(String directoryName, DirectoryItem directoryItem)
        {
            bool found = false;

            int parentID = directoryItem.parentID;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter directNameParam = new SQLiteParameter();
            directNameParam.ParameterName = "@directname";
            directNameParam.Value = directoryName;

            cmd.Parameters.Add(directNameParam);

            SQLiteParameter directIDParam = new SQLiteParameter();
            directIDParam.ParameterName = "@directid";
            directIDParam.Value = parentID;

            cmd.Parameters.Add(directIDParam);

            cmd.CommandText = "SELECT COUNT(1) FROM directories WHERE LOWER(name) LIKE LOWER(@directname) AND parentid = @directid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public List<SEAL_V2.model.Directory> getSubDirectories(int parentID)
        {
            createDirectoriesTable();

            List<SEAL_V2.model.Directory> directories = new List<SEAL_V2.model.Directory>();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter parentIDParam = new SQLiteParameter();
            parentIDParam.ParameterName = "@parentid";
            parentIDParam.Value = parentID;

            cmd.Parameters.Add(parentIDParam);

            cmd.CommandText = "SELECT * FROM directories WHERE parentid = @parentid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                directories.Add(new SEAL_V2.model.Directory(Int32.Parse(read["id"].ToString()), read["name"].ToString(), Int32.Parse(read["parentid"].ToString()), Int32.Parse(read["sequenceapplied"].ToString()), Int32.Parse(read["sequenceid"].ToString())));
            }

            return directories;
        }

        public SEAL_V2.model.Directory getDirectory(int dirID)
        {
            SEAL_V2.model.Directory found = null;

            createDirectoriesTable();

            List<SEAL_V2.model.Directory> directories = new List<SEAL_V2.model.Directory>();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter dirIDParam = new SQLiteParameter();
            dirIDParam.ParameterName = "@dirid";
            dirIDParam.Value = dirID;

            cmd.Parameters.Add(dirIDParam);

            cmd.CommandText = "SELECT * FROM directories WHERE id = @dirid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                found = new SEAL_V2.model.Directory(Int32.Parse(read["id"].ToString()), read["name"].ToString(), Int32.Parse(read["parentid"].ToString()), Int32.Parse(read["sequenceapplied"].ToString()), Int32.Parse(read["sequenceid"].ToString()));
            }

            return found;
        }

        public void updateDirectory(int id, String name, int sequenceApplied, int sequenceID)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter directoryIDParam = new SQLiteParameter();
            directoryIDParam.ParameterName = "@directoryid";
            directoryIDParam.Value = id;

            cmd.Parameters.Add(directoryIDParam);

            SQLiteParameter directoryNameParam = new SQLiteParameter();
            directoryNameParam.ParameterName = "@directoryname";
            directoryNameParam.Value = name;

            cmd.Parameters.Add(directoryNameParam);

            SQLiteParameter sequenceAppliedParam = new SQLiteParameter();
            sequenceAppliedParam.ParameterName = "@sequenceapplied";
            sequenceAppliedParam.Value = sequenceApplied;

            cmd.Parameters.Add(sequenceAppliedParam);

            SQLiteParameter sequenceIDParam = new SQLiteParameter();
            sequenceIDParam.ParameterName = "@sequenceid";
            sequenceIDParam.Value = sequenceID;

            cmd.Parameters.Add(sequenceIDParam);

            cmd.CommandText = "UPDATE directories SET name = @directoryname, sequenceapplied = @sequenceapplied, sequenceid = @sequenceid WHERE id = @directoryid";

            cmd.ExecuteNonQuery();
        }

        public bool doSubDirectoriesExist(int passedID)
        {
            bool result = false;

            List<SEAL_V2.model.Directory> temp = getSubDirectories(passedID);

            if (temp.Count > 0)
            {
                result = true;
            }

            return result;
        }

        public void deleteDirectory(int directoryID)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter idParam = new SQLiteParameter();
            idParam.ParameterName = "@directoryid";
            idParam.Value = directoryID;

            cmd.Parameters.Add(idParam);

            cmd.CommandText = "DELETE FROM directories WHERE id = @directoryid";

            cmd.ExecuteNonQuery();
        }

        public bool sequenceRequired(int dirID)
        {
            createDirectoriesTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter dirDParam = new SQLiteParameter();
            dirDParam.ParameterName = "@dirid";
            dirDParam.Value = dirID;

            cmd.Parameters.Add(dirDParam);

            cmd.CommandText = "SELECT COUNT(1) FROM directories WHERE id = @dirid AND sequenceapplied = 1";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public void createSystemsTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS systems(id INTEGER PRIMARY KEY, modelname TEXT, addnickname INTEGER, nickname TEXT, assigneddir INTEGER, directoryid INTEGER, regadded INT, regkey TEXT, regvalue TEXT)";
            cmd.ExecuteNonQuery();
        }

        public void addSystem(String modelName, int nicknameadd, String nickname, int assigneddir, int dirid, int regadded, String regkey, String regvalue)
        {
            createSystemsTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            SQLiteParameter nicknameAddParam = new SQLiteParameter();
            nicknameAddParam.ParameterName = "@nicknameadd";
            nicknameAddParam.Value = nicknameadd;

            cmd.Parameters.Add(nicknameAddParam);

            SQLiteParameter nicknameParam = new SQLiteParameter();
            nicknameParam.ParameterName = "@nickname";
            nicknameParam.Value = nickname;

            cmd.Parameters.Add(nicknameParam);

            SQLiteParameter assignDirParam = new SQLiteParameter();
            assignDirParam.ParameterName = "@assigndir";
            assignDirParam.Value = assigneddir;

            cmd.Parameters.Add(assignDirParam);

            SQLiteParameter dirIDParam = new SQLiteParameter();
            dirIDParam.ParameterName = "@dirid";
            dirIDParam.Value = dirid;

            cmd.Parameters.Add(dirIDParam);

            SQLiteParameter regAddedParam = new SQLiteParameter();
            regAddedParam.ParameterName = "@regadded";
            regAddedParam.Value = regadded;

            cmd.Parameters.Add(regAddedParam);

            SQLiteParameter regKeyParam = new SQLiteParameter();
            regKeyParam.ParameterName = "@regkey";
            regKeyParam.Value = regkey;

            cmd.Parameters.Add(regKeyParam);

            SQLiteParameter regValueParam = new SQLiteParameter();
            regValueParam.ParameterName = "@regvalue";
            regValueParam.Value = regvalue;

            cmd.Parameters.Add(regValueParam);

            cmd.CommandText = "INSERT INTO systems(modelname, addnickname, nickname, assigneddir, directoryid, regadded, regkey, regvalue) VALUES(@modelname, @nicknameadd, @nickname, @assigndir, @dirid, @regadded, @regkey, @regvalue); SELECT last_insert_rowid()";

            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT * FROM systems WHERE LOWER(modelname) LIKE LOWER(@modelname) ORDER BY id DESC LIMIT 1";

            SQLiteDataReader read = cmd.ExecuteReader();

            int initialID = 0;

            while (read.Read())
            {
                initialID = Int32.Parse(read["id"].ToString());
            }

            CurrentSystem.initialID = initialID;
            CurrentSystem.machineID = initialID;
        }

        public List<SystemObject> getSystems()
        {
            createSystemsTable();

            List<SystemObject> list = new List<SystemObject>();

            using var cmd = new SQLiteCommand(connection);

            //If multiple system models exist then pull from latest...

            cmd.CommandText = "SELECT * FROM systems";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                SystemObject systemObject = new SystemObject();

                systemObject.id = Int32.Parse(read["id"].ToString());
                systemObject.modelname = read["modelname"].ToString();
                systemObject.addnickname = Int32.Parse(read["addnickname"].ToString());
                systemObject.nickname = read["nickname"].ToString();
                systemObject.assigneddir = Int32.Parse(read["assigneddir"].ToString());
                systemObject.DirID = Int32.Parse(read["directoryid"].ToString());
                systemObject.regadded = Int32.Parse(read["regadded"].ToString());
                systemObject.regkey = read["regkey"].ToString();
                systemObject.regvalue = read["regvalue"].ToString();

                list.Add(systemObject);
            }

            return list;
        }

        public SystemObject getSystem(int systemID)
        {
            createSystemsTable();

            SystemObject system = null;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter machineIDParam = new SQLiteParameter();
            machineIDParam.ParameterName = "@machineID";
            machineIDParam.Value = systemID;

            cmd.Parameters.Add(machineIDParam);

            cmd.CommandText = "SELECT * FROM systems WHERE id = @machineID";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                SystemObject systemObject = new SystemObject();

                systemObject.id = Int32.Parse(read["id"].ToString());
                systemObject.modelname = read["modelname"].ToString();
                systemObject.addnickname = Int32.Parse(read["addnickname"].ToString());
                systemObject.nickname = read["nickname"].ToString();
                systemObject.assigneddir = Int32.Parse(read["assigneddir"].ToString());
                systemObject.DirID = Int32.Parse(read["directoryid"].ToString());
                systemObject.regadded = Int32.Parse(read["regadded"].ToString());
                systemObject.regkey = read["regkey"].ToString();
                systemObject.regvalue = read["regvalue"].ToString();

                system = systemObject;
            }

            return system;
        }

        public void updateSystem(int machineID, String modelName, int nicknameadd, String nickname, int assigneddir, int dirid, int regadded, String regkey, String regvalue)
        {
            createSystemsTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter machineIDParam = new SQLiteParameter();
            machineIDParam.ParameterName = "@machineID";
            machineIDParam.Value = machineID;

            cmd.Parameters.Add(machineIDParam);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            SQLiteParameter nicknameAddParam = new SQLiteParameter();
            nicknameAddParam.ParameterName = "@nicknameadd";
            nicknameAddParam.Value = nicknameadd;

            cmd.Parameters.Add(nicknameAddParam);

            SQLiteParameter nicknameParam = new SQLiteParameter();
            nicknameParam.ParameterName = "@nickname";
            nicknameParam.Value = nickname;

            cmd.Parameters.Add(nicknameParam);

            SQLiteParameter assignDirParam = new SQLiteParameter();
            assignDirParam.ParameterName = "@assignedir";
            assignDirParam.Value = assigneddir;

            cmd.Parameters.Add(assignDirParam);

            SQLiteParameter dirIDParam = new SQLiteParameter();
            dirIDParam.ParameterName = "@dirid";
            dirIDParam.Value = dirid;

            cmd.Parameters.Add(dirIDParam);

            SQLiteParameter regAddedParam = new SQLiteParameter();
            regAddedParam.ParameterName = "@regadded";
            regAddedParam.Value = regadded;

            cmd.Parameters.Add(regAddedParam);

            SQLiteParameter regKeyParam = new SQLiteParameter();
            regKeyParam.ParameterName = "@regkey";
            regKeyParam.Value = regkey;

            cmd.Parameters.Add(regKeyParam);

            SQLiteParameter regValueParam = new SQLiteParameter();
            regValueParam.ParameterName = "@regvalue";
            regValueParam.Value = regvalue;

            cmd.Parameters.Add(regValueParam);

            cmd.CommandText = "UPDATE systems SET modelname = @modelname, addnickname = @nicknameadd, nickname = @nickname, assigneddir = @assignedir, directoryid = @dirid, regadded = @regadded, regkey = @regkey, regvalue = @regvalue WHERE id = @machineID";

            cmd.ExecuteNonQuery();
        }

        public void loadLocalSystem(String modelName)
        {
            createSystemsTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@systemid";
            modelNameParam.Value = getLatestSystem(modelName);

            cmd.Parameters.Add(modelNameParam);

            //If multiple system models exist then pull from latest...

            cmd.CommandText = "SELECT * FROM systems WHERE id = @systemid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                CurrentSystem.machineID = Int32.Parse(read["id"].ToString());
                CurrentSystem.nicknameAdded = Int32.Parse(read["addnickname"].ToString());
                CurrentSystem.nickname = read["nickname"].ToString();
                CurrentSystem.dirAdded = Int32.Parse(read["assigneddir"].ToString());
                CurrentSystem.dirID = Int32.Parse(read["directoryid"].ToString());
                CurrentSystem.regadded = Int32.Parse(read["regadded"].ToString());
                CurrentSystem.regKey = read["regkey"].ToString();
                CurrentSystem.regValue = read["regvalue"].ToString();
            }
        }

        public int getLatestSystem(String modelName)
        {
            int i = 0;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            cmd.CommandText = "SELECT * FROM systems WHERE LOWER(modelname) LIKE LOWER(@modelname) ORDER BY id DESC LIMIT 1";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                i = Int32.Parse(read["id"].ToString());
            }

            return i;
        }


        public bool doesSystemExist(String modelName)
        {
            createSystemsTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            cmd.CommandText = "SELECT COUNT(1) FROM systems WHERE LOWER(modelname) LIKE LOWER(@modelname)";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool doesUnassignedSystemExist(String modelName)
        {
            createSystemsTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            cmd.CommandText = "SELECT COUNT(1) FROM systems WHERE LOWER(modelname) LIKE LOWER(@modelname) AND directoryid = 0";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool isCurrentSystemUnassigned(int machineid)
        {
            createSystemsTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter machineidParam = new SQLiteParameter();
            machineidParam.ParameterName = "@machineid";
            machineidParam.Value = machineid;

            cmd.Parameters.Add(machineidParam);

            cmd.CommandText = "SELECT COUNT(1) FROM systems WHERE machineid = @machineid AND assigneddir = 0";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool doesSystemModelExistinDir(String modelName, int dirID)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            SQLiteParameter dirIDParam = new SQLiteParameter();
            dirIDParam.ParameterName = "@dirID";
            dirIDParam.Value = dirID;

            cmd.Parameters.Add(dirIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM systems WHERE LOWER(modelname) LIKE LOWER(@modelname) AND directoryid = @dirID";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public int howManyModelInstances(String modelName)
        {
            createSystemsTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter modelNameParam = new SQLiteParameter();
            modelNameParam.ParameterName = "@modelname";
            modelNameParam.Value = modelName;

            cmd.Parameters.Add(modelNameParam);

            cmd.CommandText = "SELECT COUNT(*) FROM systems WHERE modelname = @modelname";


            var result = cmd.ExecuteScalar();

            //cmd.ExecuteNonQuery();

            return Convert.ToInt32(result);
        }

        public void deleteSystem(int machineid)
        {
            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter idParam = new SQLiteParameter();
            idParam.ParameterName = "@machineid";
            idParam.Value = machineid;

            cmd.Parameters.Add(idParam);

            cmd.CommandText = "DELETE FROM systems WHERE id = @machineid";

            cmd.ExecuteNonQuery();
        }

        public void createCaptureHistoryTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS captures(id INTEGER PRIMARY KEY, capturename TEXT, systemid INTEGER, sequenceid INTEGER, currentstep INTEGER, status INTEGER)";
            cmd.ExecuteNonQuery();
        }

        public bool captureExists(int systemID, int sequenceID)
        {
            createCaptureHistoryTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter systemIDParam = new SQLiteParameter();
            systemIDParam.ParameterName = "@systemID";
            systemIDParam.Value = systemID;

            cmd.Parameters.Add(systemIDParam);

            SQLiteParameter sequenceIDParam = new SQLiteParameter();
            sequenceIDParam.ParameterName = "@sequenceID";
            sequenceIDParam.Value = sequenceID;

            cmd.Parameters.Add(sequenceIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM captures WHERE systemid = @systemid AND sequenceid = @sequenceid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool captureExists(int systemID)
        {
            createCaptureHistoryTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter systemIDParam = new SQLiteParameter();
            systemIDParam.ParameterName = "@systemID";
            systemIDParam.Value = systemID;

            cmd.Parameters.Add(systemIDParam);

            cmd.CommandText = "SELECT COUNT(1) FROM captures WHERE systemid = @systemid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool captureNameExistForSystem(String name, int systemID)
        {
            createCaptureHistoryTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter nameParam = new SQLiteParameter();
            nameParam.ParameterName = "@scapturename";
            nameParam.Value = name;

            cmd.Parameters.Add(nameParam);

            SQLiteParameter systemIDParam = new SQLiteParameter();
            systemIDParam.ParameterName = "@systemID";
            systemIDParam.Value = systemID;

            cmd.Parameters.Add(systemIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM captures WHERE capturename = @scapturename AND systemid = @systemID";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public int saveCapture(String captureName, int systemID, int sequenceID, int currentStep, int status)
        {
            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureNameParam = new SQLiteParameter();
            captureNameParam.ParameterName = "@capturename";
            captureNameParam.Value = captureName;

            cmd.Parameters.Add(captureNameParam);

            SQLiteParameter systemidParam = new SQLiteParameter();
            systemidParam.ParameterName = "@systemid";
            systemidParam.Value = systemID;

            cmd.Parameters.Add(systemidParam);

            SQLiteParameter sequenceParam = new SQLiteParameter();
            sequenceParam.ParameterName = "@sequenceid";
            sequenceParam.Value = sequenceID;

            cmd.Parameters.Add(sequenceParam);

            SQLiteParameter currentStepParam = new SQLiteParameter();
            currentStepParam.ParameterName = "@curstep";
            currentStepParam.Value = currentStep;

            cmd.Parameters.Add(currentStepParam);

            SQLiteParameter statusParam = new SQLiteParameter();
            statusParam.ParameterName = "@status";
            statusParam.Value = status;

            cmd.Parameters.Add(statusParam);

            cmd.CommandText = "INSERT INTO captures(capturename, systemid, sequenceid, currentstep, status) VALUES(@capturename, @systemid, @sequenceid, @curstep, @status); SELECT last_insert_rowid()";

            var result = cmd.ExecuteScalar();

            //cmd.ExecuteNonQuery();

            return Convert.ToInt32(result);
        }

        public Capture getCapture(int captureid)
        {
            Capture found = null;

            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureid;

            cmd.Parameters.Add(captureIDParam);

            cmd.CommandText = "SELECT * FROM captures WHERE id = @captureid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                found = new Capture(Int32.Parse(read["id"].ToString()), read["capturename"].ToString(), Int32.Parse(read["systemid"].ToString()), Int32.Parse(read["sequenceid"].ToString()), Int32.Parse(read["currentstep"].ToString()), Int32.Parse(read["status"].ToString()));
            }

            return found;
        }

        public List<Capture> getCaptures(int systemID)
        {
            List<Capture> list = new List<Capture>();

            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter systemIDParam = new SQLiteParameter();
            systemIDParam.ParameterName = "@systemid";
            systemIDParam.Value = systemID;

            cmd.Parameters.Add(systemIDParam);

            cmd.CommandText = "SELECT * FROM captures WHERE systemid = @systemid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                list.Add(new Capture(Int32.Parse(read["id"].ToString()), read["capturename"].ToString(), Int32.Parse(read["systemid"].ToString()), Int32.Parse(read["sequenceid"].ToString()), Int32.Parse(read["currentstep"].ToString()), Int32.Parse(read["status"].ToString())));
            }

            return list;
        }

        public Capture getLatestCapture(int machineID, int sequenceID)
        {
            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter machineIDParam = new SQLiteParameter();
            machineIDParam.ParameterName = "@machineid";
            machineIDParam.Value = machineID;

            cmd.Parameters.Add(machineIDParam);

            SQLiteParameter sequenceIDParam = new SQLiteParameter();
            sequenceIDParam.ParameterName = "@sequenceid";
            sequenceIDParam.Value = sequenceID;

            cmd.Parameters.Add(sequenceIDParam);

            cmd.CommandText = "SELECT * FROM captures WHERE systemid = @machineID AND sequenceid = @sequenceid ORDER BY id DESC LIMIT 1";

            SQLiteDataReader read = cmd.ExecuteReader();

            int i = 0;

            while (read.Read())
            {
                i = Int32.Parse(read["id"].ToString());
            }

            return getCapture(i);
        }

        public Capture getPreviousCapture(int machineID)
        {
            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter machineIDParam = new SQLiteParameter();
            machineIDParam.ParameterName = "@machineid";
            machineIDParam.Value = machineID;

            cmd.Parameters.Add(machineIDParam);

            cmd.CommandText = "SELECT * FROM captures WHERE systemid = @machineID ORDER BY id DESC LIMIT 2";

            SQLiteDataReader read = cmd.ExecuteReader();

            int i = 0;

            while (read.Read())
            {
                i = Int32.Parse(read["id"].ToString());
            }

            return getCapture(i);
        }

        public Capture getLatestCapture(int machineID)
        {
            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter machineIDParam = new SQLiteParameter();
            machineIDParam.ParameterName = "@machineid";
            machineIDParam.Value = machineID;

            cmd.Parameters.Add(machineIDParam);


            cmd.CommandText = "SELECT * FROM captures WHERE systemid = @machineID ORDER BY id DESC LIMIT 1";

            SQLiteDataReader read = cmd.ExecuteReader();

            int i = 0;

            while (read.Read())
            {
                i = Int32.Parse(read["id"].ToString());
            }

            return getCapture(i);
        }

        public void updateCapture(int captureid, int currentstep, int status)
        {
            createCaptureHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureid;

            cmd.Parameters.Add(captureIDParam);

            SQLiteParameter currentstepParam = new SQLiteParameter();
            currentstepParam.ParameterName = "@currentstep";
            currentstepParam.Value = currentstep;

            cmd.Parameters.Add(currentstepParam);

            SQLiteParameter statusParam = new SQLiteParameter();
            statusParam.ParameterName = "@status";
            statusParam.Value = status;

            cmd.Parameters.Add(statusParam);

            cmd.CommandText = "UPDATE captures SET currentstep = @currentstep, status = @status WHERE id = @captureid";

            cmd.ExecuteNonQuery();
        }

        public void createActionHistoryTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS history(id INTEGER PRIMARY KEY, groupid INTEGER, groupname TEXT, username TEXT, model TEXT, serial TEXT, captureid INTEGER, capturename TEXT, currentstep INTEGER, status TEXT, statusColor INTEGER)";
            cmd.ExecuteNonQuery();
        }

        public void saveAction(int groupid, String groupname, String username, String modelName, String serial, int captureID, String captureName, int currentStep, String status, int statusColor)
        {
            createActionHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter groupIDParam = new SQLiteParameter();
            groupIDParam.ParameterName = "@groupid";
            groupIDParam.Value = groupid;

            cmd.Parameters.Add(groupIDParam);

            SQLiteParameter groupNameParam = new SQLiteParameter();
            groupNameParam.ParameterName = "@groupname";
            groupNameParam.Value = groupname;

            cmd.Parameters.Add(groupNameParam);

            SQLiteParameter userNameParam = new SQLiteParameter();
            userNameParam.ParameterName = "@username";
            userNameParam.Value = username;

            cmd.Parameters.Add(userNameParam);

            SQLiteParameter modelnameParam = new SQLiteParameter();
            modelnameParam.ParameterName = "@modelname";
            modelnameParam.Value = modelName;

            cmd.Parameters.Add(modelnameParam);

            SQLiteParameter serialParam = new SQLiteParameter();
            serialParam.ParameterName = "@serial";
            serialParam.Value = serial;

            cmd.Parameters.Add(serialParam);

            SQLiteParameter captureNameParam = new SQLiteParameter();
            captureNameParam.ParameterName = "@capturename";
            captureNameParam.Value = captureName;

            cmd.Parameters.Add(captureNameParam);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);

            SQLiteParameter currentStepParam = new SQLiteParameter();
            currentStepParam.ParameterName = "@step";
            currentStepParam.Value = currentStep + 1;

            cmd.Parameters.Add(currentStepParam);

            SQLiteParameter statusParam = new SQLiteParameter();
            statusParam.ParameterName = "@status";
            statusParam.Value = status;

            cmd.Parameters.Add(statusParam);

            SQLiteParameter statusColorParam = new SQLiteParameter();
            statusColorParam.ParameterName = "@color";
            statusColorParam.Value = statusColor;

            cmd.Parameters.Add(statusColorParam);

            cmd.CommandText = "INSERT INTO history(groupid, groupname, username, model, serial, captureid, capturename, currentstep, status, statuscolor) VALUES(@groupid, @groupname, @username, @modelname, @serial, @captureid, @capturename, @step, @status, @color)";

            cmd.ExecuteNonQuery();
        }

        public List<History> getHistory()
        {
            List<History> list = new List<History>();

            createActionHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "SELECT * FROM history";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                list.Add(new History(Int32.Parse(read["id"].ToString()), Int32.Parse(read["groupid"].ToString()), read["groupname"].ToString(), read["username"].ToString(), read["model"].ToString(), read["serial"].ToString(), Int32.Parse(read["captureid"].ToString()), read["capturename"].ToString(), Int32.Parse(read["currentstep"].ToString()), read["status"].ToString(), Int32.Parse(read["statuscolor"].ToString())));
            }

            list.Reverse();

            return list;
        }

        public List<History> getHistory(int captureID)
        {
            List<History> list = new List<History>();

            createActionHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);

            cmd.CommandText = "SELECT * FROM history WHERE captureID = @captureid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                list.Add(new History(Int32.Parse(read["id"].ToString()), Int32.Parse(read["groupid"].ToString()), read["groupname"].ToString(), read["username"].ToString(), read["model"].ToString(), read["serial"].ToString(), Int32.Parse(read["captureid"].ToString()), read["capturename"].ToString(), Int32.Parse(read["currentstep"].ToString()), read["status"].ToString(), Int32.Parse(read["statuscolor"].ToString())));
            }

            list.Reverse();

            return list;
        }

        public void createSoftwareTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS software(id INTEGER PRIMARY KEY, softwarename TEXT, softwareversion TEXT, softwarevendor TEXT, softwaretype TEXT, added INTEGER, location TEXT, regadd INTEGER, regkey TEXT, captureid INTEGER, visible INT)";
            cmd.ExecuteNonQuery();
        }

        public void cleanSoftwareTableWithCapture(int captureID)
        {
            createSoftwareTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);

            cmd.CommandText = "DELETE FROM software WHERE captureid = @captureid";
            cmd.ExecuteNonQuery();
        }

        public void findWMISoftware(int captureID)
        {
            createSoftwareTable();

            using var cmd = new SQLiteCommand(connection);

            //GET APPLICATIONS
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Product");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    if (wmi.GetPropertyValue("Name") != null && wmi.GetPropertyValue("Version") != null && wmi.GetPropertyValue("Vendor") != null)
                    {
                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwareType, added, regadd, captureid, visible) VALUES('" + wmi.GetPropertyValue("Name").ToString() + "', '" + wmi.GetPropertyValue("Version").ToString() + "', '" + wmi.GetPropertyValue("Vendor").ToString() + "', 'APPLICATION', " + "0, 0, " + captureID + ", 0)";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        //NEED TO LOG ERROR
                    }

                }
                catch (NullReferenceException e)
                {
                    var error = e;
                }
            }

            //GET DRIVERS
            ManagementObjectSearcher driverSearch = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPSignedDriver");
            foreach (ManagementObject wmi in driverSearch.Get())
            {
                try
                {
                    if (wmi.GetPropertyValue("Description") != null && wmi.GetPropertyValue("DriverVersion") != null && wmi.GetPropertyValue("Manufacturer") != null)
                    {
                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwareType, added, regadd, captureid, visible) VALUES('" + wmi.GetPropertyValue("Description").ToString() + "', '" + wmi.GetPropertyValue("DriverVersion").ToString() + "', '" + wmi.GetPropertyValue("Manufacturer").ToString() + "', 'DRIVER', " + "0, 0, " + captureID + ", 0)";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (NullReferenceException e)
                {
                    var error = e;
                }
            }

            //GET SECURITY KB UPDATES
            ManagementObjectSearcher kBSearch = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_QuickFixEngineering");
            foreach (ManagementObject wmi in kBSearch.Get())
            {
                try
                {
                    if (wmi.GetPropertyValue("HotFixID") != null)
                    {
                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwareType, added, regadd, captureid, visible) VALUES('Security Update', '" + wmi.GetPropertyValue("HotFixID").ToString() + "', 'Microsoft Corporation', 'SECURITY', " + "0, 0, " + captureID + ", 0)";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (NullReferenceException e)
                {
                    var error = e;
                }
            }
        }

        public void findRegistrySoftware(int captureID)
        {
            regex.Regex rgx = new regex.Regex(@"KB\d{5}\d*", regex.RegexOptions.IgnoreCase);
            regex.Regex edgeRgx = new regex.Regex(@"S-1-5-21-\d*-\d*-\d*-\d*_Classes", regex.RegexOptions.IgnoreCase);
            regex.Regex microsoftEdgeRgx = new regex.Regex(@"Microsoft.MicrosoftEdge", regex.RegexOptions.IgnoreCase);
            regex.Regex edgeVersion = new regex.Regex(@"\d*[.]\d*[.]\d*[.]\d*", regex.RegexOptions.IgnoreCase);

            String regLoc64 = "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            String regWow64 = "Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            String windowsVersion = "Software\\Microsoft\\Windows NT\\CurrentVersion";


            using (var rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (var key = rootKey.OpenSubKey(regLoc64, false))
                {
                    using var cmd = new SQLiteCommand(connection);

                    foreach (String v in key.GetSubKeyNames())
                    {
                        try
                        {
                            regex.MatchCollection matches = rgx.Matches(v.ToString());

                            //Gets kb from key
                            if (matches.Count > 0)
                            {
                                foreach (regex.Match matchFound in matches)
                                {
                                    if (!dupeExists("Security Update", matchFound.Value, captureID))
                                    {
                                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwaretype, added, regadd, captureid, visible) VALUES('Security Update', '" + matchFound.Value + "', 'Microsoft Corporation', 'SECURITY', " + "0, 0, " + captureID + ", 0)";
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                            }
                            else
                            {
                                RegistryKey productKey = key.OpenSubKey(v);

                                if (productKey != null)
                                {

                                    String displayName = Convert.ToString(productKey.GetValue("DisplayName"));
                                    String displayVersion = Convert.ToString(productKey.GetValue("DisplayVersion"));
                                    String publisher = Convert.ToString(productKey.GetValue("Publisher"));

                                    if (!displayName.Equals(""))
                                    {
                                        if (!dupeExists(displayName, displayVersion, captureID))
                                        {
                                            cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwaretype, added, regadd, captureid, visible) VALUES('" + displayName + "', '" + displayVersion + "', '" + publisher + "', 'REGISTRY', " + "0, 0, " + captureID + ", + 0)";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                        catch (SQLiteException e)
                        {

                        }
                    }
                }

                //Check wow64 keys
                using (var key = rootKey.OpenSubKey(regWow64, false))
                {
                    using var cmd = new SQLiteCommand(connection);

                    foreach (String v in key.GetSubKeyNames())
                    {
                        try
                        {
                            regex.MatchCollection matches = rgx.Matches(v.ToString());

                            //Gets kb from key
                            if (matches.Count > 0)
                            {
                                foreach (regex.Match matchFound in matches)
                                {
                                    if (!dupeExists("Security Update", matchFound.Value, captureID))
                                    {
                                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwaretype, added, regadd, captureid, visible) VALUES('Security Update', '" + matchFound.Value + "', 'Microsoft Corporation', 'SECURITY', " + "0, 0, " + captureID + ", 0)";
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                            }
                            else
                            {
                                RegistryKey productKey = key.OpenSubKey(v);

                                if (productKey != null)
                                {

                                    String displayName = Convert.ToString(productKey.GetValue("DisplayName"));
                                    String displayVersion = Convert.ToString(productKey.GetValue("DisplayVersion"));
                                    String publisher = Convert.ToString(productKey.GetValue("Publisher"));

                                    if (!displayName.Equals(""))
                                    {
                                        if (!dupeExists(displayName, displayVersion, captureID))
                                        {
                                            cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwaretype, added, regadd, captureid, visible) VALUES('" + displayName + "', '" + displayVersion + "', '" + publisher + "', 'REGISTRY', " + "0, 0, " + captureID + ", + 0)";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                        catch (SQLiteException e)
                        {

                        }
                    }
                }

                RegistryKey windowsKey = Registry.LocalMachine.OpenSubKey(windowsVersion);

                if (windowsKey != null)
                {
                    String productName = Convert.ToString(windowsKey.GetValue("ProductName"));
                    String servicePack = Convert.ToString(windowsKey.GetValue("CSDVersion"));
                    String mainVersion = Convert.ToString(windowsKey.GetValue("ReleaseID"));
                    String bitVersion;

                    using var cmd = new SQLiteCommand(connection);

                    //THIS MIGHT NOT BE NEEDED IF ALL MACHINES ARE 64BIT
                    if (Environment.Is64BitOperatingSystem)
                    {
                        bitVersion = "64 bit";
                    }
                    else
                    {
                        bitVersion = "32 bit";
                    }

                    String mainBuild = Convert.ToString(windowsKey.GetValue("CurrentBuild"));
                    var subBuild = windowsKey.GetValue("UBR");

                    //MAY NOT BE NEEDED EITHER
                    if (servicePack != "")
                    {
                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwareType, added, regadd, captureid, visible) VALUES('" + productName + "; " + servicePack + "; " + bitVersion + "', 'V " + mainVersion + " Build " + mainBuild + "." + subBuild + "', 'Microsoft', 'REGISTRY', " + "0, 0, " + captureID + ", + 0)";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwareType, added, regadd, captureid, visible) VALUES('" + productName + "; " + bitVersion + "', 'V " + mainVersion + " Build " + mainBuild + "." + subBuild + "', 'Microsoft', 'REGISTRY', " + "0, 0, " + captureID + ", + 0)";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Software> getSoftware(int captureID)
        {
            List<Software> list = new List<Software>();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT * FROM software WHERE captureid = @captureid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                Software software = new Software();
                software.ID = Int32.Parse(read["id"].ToString());
                software.SoftwareName = read["softwarename"].ToString();
                software.SoftwareVersion = read["softwareversion"].ToString();
                software.SoftwareVendor = read["softwarevendor"].ToString();
                software.SoftwareType = read["softwaretype"].ToString();
                software.Added = Int32.Parse(read["added"].ToString());
                software.Location = read["location"].ToString();
                software.RegAdd = Int32.Parse(read["regadd"].ToString());
                software.RegKey = read["regkey"].ToString();
                software.CaptureID = Int32.Parse(read["captureid"].ToString());
                software.Visible = Int32.Parse(read["visible"].ToString());

                list.Add(software);
            }

            return list;
        }

        public void addSoftware(Software softwareItem)
        {
            createActionHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter softwareNameParam = new SQLiteParameter();
            softwareNameParam.ParameterName = "@softwarename";
            softwareNameParam.Value = softwareItem.SoftwareName;

            cmd.Parameters.Add(softwareNameParam);

            SQLiteParameter softwareVersionParam = new SQLiteParameter();
            softwareVersionParam.ParameterName = "@softwareversion";
            softwareVersionParam.Value = softwareItem.SoftwareVersion;

            cmd.Parameters.Add(softwareVersionParam);

            SQLiteParameter softwareVendorParam = new SQLiteParameter();
            softwareVendorParam.ParameterName = "@softwarevendor";
            softwareVendorParam.Value = softwareItem.SoftwareVendor;

            cmd.Parameters.Add(softwareVendorParam);

            SQLiteParameter softwareTypeParam = new SQLiteParameter();
            softwareTypeParam.ParameterName = "@softwaretype";
            softwareTypeParam.Value = softwareItem.SoftwareType;

            cmd.Parameters.Add(softwareTypeParam);

            SQLiteParameter captureidParam = new SQLiteParameter();
            captureidParam.ParameterName = "@captureid";
            captureidParam.Value = softwareItem.CaptureID;

            cmd.Parameters.Add(captureidParam);

            SQLiteParameter addedParam = new SQLiteParameter();
            addedParam.ParameterName = "@added";
            addedParam.Value = softwareItem.Added;

            cmd.Parameters.Add(addedParam);

            SQLiteParameter locationParam = new SQLiteParameter();
            locationParam.ParameterName = "@location";
            locationParam.Value = softwareItem.Location;

            cmd.Parameters.Add(locationParam);

            SQLiteParameter regAddParam = new SQLiteParameter();
            regAddParam.ParameterName = "@regadd";
            regAddParam.Value = softwareItem.RegAdd;

            cmd.Parameters.Add(regAddParam);

            SQLiteParameter regKeyParam = new SQLiteParameter();
            regKeyParam.ParameterName = "@regkey";
            regKeyParam.Value = softwareItem.RegKey;

            cmd.Parameters.Add(regKeyParam);

            SQLiteParameter visibleParam = new SQLiteParameter();
            visibleParam.ParameterName = "@visible";
            visibleParam.Value = softwareItem.Visible;

            cmd.Parameters.Add(visibleParam);

            cmd.CommandText = "INSERT INTO software(softwarename, softwareversion, softwarevendor, softwaretype, captureid, added, location, regadd, regkey, visible) VALUES(@softwarename, @softwareversion, @softwarevendor, @softwaretype, @captureid, @added, @location, @regadd, @regkey, @visible)";

            cmd.ExecuteNonQuery();
        }

        public List<Software> getPreviousAddedSoftware(int previouseCaptureID)
        {
            List<Software> list = new List<Software>();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = previouseCaptureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT * FROM software WHERE captureid = @captureid AND added = 1";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                Software software = new Software();
                software.ID = Int32.Parse(read["id"].ToString());
                software.SoftwareName = read["softwarename"].ToString();
                software.SoftwareVersion = read["softwareversion"].ToString();
                software.SoftwareVendor = read["softwarevendor"].ToString();
                software.SoftwareType = read["softwaretype"].ToString();
                software.Added = Int32.Parse(read["added"].ToString());
                software.Location = read["location"].ToString();
                software.RegAdd = Int32.Parse(read["regadd"].ToString());
                software.RegKey = read["regkey"].ToString();
                software.CaptureID = Int32.Parse(read["captureid"].ToString());
                software.Visible = Int32.Parse(read["visible"].ToString());

                list.Add(software);
            }

            return list;
        }

        public bool isVisible(Software software, int previousCaptureID)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter nameParam = new SQLiteParameter();
            nameParam.ParameterName = "@softwarename";
            nameParam.Value = software.SoftwareName;

            cmd.Parameters.Add(nameParam);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = previousCaptureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM software WHERE softwarename = @softwarename AND captureid = @captureid AND visible = 1";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public bool dupeExists(String name, String version, int currentCaptureID)
        {
            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter nameParam = new SQLiteParameter();
            nameParam.ParameterName = "@softwarename";
            nameParam.Value = name;

            cmd.Parameters.Add(nameParam);

            SQLiteParameter versionParam = new SQLiteParameter();
            versionParam.ParameterName = "@softwareversion";
            versionParam.Value = version;

            cmd.Parameters.Add(versionParam);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = currentCaptureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM software WHERE softwarename = @softwarename AND softwareversion = @softwareversion AND captureid = @captureid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public void updateSoftware(Software software)
        {
            createActionHistoryTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter softwareIDParam = new SQLiteParameter();
            softwareIDParam.ParameterName = "@softwareid";
            softwareIDParam.Value = software.ID;

            cmd.Parameters.Add(softwareIDParam);

            SQLiteParameter softwareNameParam = new SQLiteParameter();
            softwareNameParam.ParameterName = "@softwarename";
            softwareNameParam.Value = software.SoftwareName;

            cmd.Parameters.Add(softwareNameParam);

            SQLiteParameter softwareVersionParam = new SQLiteParameter();
            softwareVersionParam.ParameterName = "@softwareversion";
            softwareVersionParam.Value = software.SoftwareVersion;

            cmd.Parameters.Add(softwareVersionParam);

            SQLiteParameter softwareVendorParam = new SQLiteParameter();
            softwareVendorParam.ParameterName = "@softwarevendor";
            softwareVendorParam.Value = software.SoftwareVendor;

            cmd.Parameters.Add(softwareVendorParam);

            SQLiteParameter softwareTypeParam = new SQLiteParameter();
            softwareTypeParam.ParameterName = "@softwaretype";
            softwareTypeParam.Value = software.SoftwareType;

            cmd.Parameters.Add(softwareTypeParam);

            SQLiteParameter captureidParam = new SQLiteParameter();
            captureidParam.ParameterName = "@captureid";
            captureidParam.Value = software.CaptureID;

            cmd.Parameters.Add(captureidParam);

            SQLiteParameter addedParam = new SQLiteParameter();
            addedParam.ParameterName = "@added";
            addedParam.Value = software.Added;

            cmd.Parameters.Add(addedParam);

            SQLiteParameter locationParam = new SQLiteParameter();
            locationParam.ParameterName = "@location";
            locationParam.Value = software.Location;

            cmd.Parameters.Add(locationParam);

            SQLiteParameter regAddParam = new SQLiteParameter();
            regAddParam.ParameterName = "@regadd";
            regAddParam.Value = software.RegAdd;

            cmd.Parameters.Add(regAddParam);

            SQLiteParameter regKeyParam = new SQLiteParameter();
            regKeyParam.ParameterName = "@regkey";
            regKeyParam.Value = software.RegKey;

            cmd.Parameters.Add(regKeyParam);

            SQLiteParameter visibleParam = new SQLiteParameter();
            visibleParam.ParameterName = "@visible";
            visibleParam.Value = software.Visible;

            cmd.Parameters.Add(visibleParam);

            cmd.CommandText = "UPDATE software SET softwarename = @softwarename, softwareversion = @softwareversion, softwarevendor = @softwarevendor, softwaretype = @softwaretype, captureid = @captureid, added = @added, location = @location, regadd = @regadd, regkey = @regkey, visible = @visible WHERE id = @softwareid";

            cmd.ExecuteNonQuery();
        }

        public bool doesSoftwareExistInCapture(int captureID)
        {
            createSoftwareTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM software WHERE captureid = @captureid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public void wipeTempVerify()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "DELETE FROM software WHERE captureid = 0";

            cmd.ExecuteNonQuery();
        }

        public void createSoftwareCompareTable()
        {
            using var cmd = new SQLiteCommand(connection);

            cmd.CommandText = "CREATE TABLE IF NOT EXISTS softwarecompare(id INTEGER PRIMARY KEY, softwarename TEXT, version TEXT, foundversion TEXT, vendor TEXT, type TEXT, captureid INTEGER, visible INTEGER, comparison INT)";
            cmd.ExecuteNonQuery();
        }

        public void cleanSoftwareCaptureTable(int captureID)
        {
            createSoftwareCompareTable();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);

            cmd.CommandText = "DELETE FROM softwarecompare WHERE captureid = @captureid";

            cmd.ExecuteNonQuery();
        }

        public bool doesSoftwareCompareExistInCapture(int captureID)
        {
            createSoftwareTable();

            bool found = false;

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT COUNT(1) FROM softwarecompare WHERE captureid = @captureid";

            var result = cmd.ExecuteScalar();
            int i = Convert.ToInt32(result);

            if (i != 0)
            {
                found = true;
            }

            return found;
        }

        public void saveSoftwareCompare(List<SoftwareCompare> list)
        {
            createSoftwareCompareTable();

            using var cmd = new SQLiteCommand(connection);

            foreach (SoftwareCompare item in list)
            {
                SQLiteParameter softwareNameParam = new SQLiteParameter();
                softwareNameParam.ParameterName = "@softwarename";
                softwareNameParam.Value = item.SoftwareName;

                cmd.Parameters.Add(softwareNameParam);

                SQLiteParameter softwareVersionParam = new SQLiteParameter();
                softwareVersionParam.ParameterName = "@version";
                softwareVersionParam.Value = item.Version;

                cmd.Parameters.Add(softwareVersionParam);

                SQLiteParameter softwareFoundVersionParam = new SQLiteParameter();
                softwareFoundVersionParam.ParameterName = "@foundversion";
                softwareFoundVersionParam.Value = item.FoundVersion;

                cmd.Parameters.Add(softwareFoundVersionParam);

                SQLiteParameter softwareVendorParam = new SQLiteParameter();
                softwareVendorParam.ParameterName = "@vendor";
                softwareVendorParam.Value = item.Vendor;

                cmd.Parameters.Add(softwareVendorParam);

                SQLiteParameter softwareTypeParam = new SQLiteParameter();
                softwareTypeParam.ParameterName = "@type";
                softwareTypeParam.Value = item.Type;

                cmd.Parameters.Add(softwareTypeParam);

                SQLiteParameter captureIDParam = new SQLiteParameter();
                captureIDParam.ParameterName = "@captureid";
                captureIDParam.Value = item.CaptureID;

                cmd.Parameters.Add(captureIDParam);

                SQLiteParameter visibleParam = new SQLiteParameter();
                visibleParam.ParameterName = "@visible";
                visibleParam.Value = item.Visible;

                cmd.Parameters.Add(visibleParam);

                SQLiteParameter comaprisonParam = new SQLiteParameter();
                comaprisonParam.ParameterName = "@comparison";
                comaprisonParam.Value = item.Comparison;

                cmd.Parameters.Add(comaprisonParam);

                cmd.CommandText = "INSERT INTO softwarecompare(softwarename, version, foundversion, vendor, type, captureid, visible, comparison) VALUES(@softwarename, @version, @foundversion, @vendor, @type, @captureid, @visible, @comparison)";

                cmd.ExecuteNonQuery();
            }
        }

        public List<SoftwareCompare> getSoftwareCompare(int captureID)
        {
            List<SoftwareCompare> list = new List<SoftwareCompare>();

            using var cmd = new SQLiteCommand(connection);

            SQLiteParameter captureIDParam = new SQLiteParameter();
            captureIDParam.ParameterName = "@captureid";
            captureIDParam.Value = captureID;

            cmd.Parameters.Add(captureIDParam);


            cmd.CommandText = "SELECT * FROM softwarecompare WHERE captureid = @captureid";

            SQLiteDataReader read = cmd.ExecuteReader();

            while (read.Read())
            {
                SoftwareCompare item = new SoftwareCompare();

                item.ID = Int32.Parse(read["id"].ToString());
                item.SoftwareName = read["softwarename"].ToString();
                item.Version = read["version"].ToString();
                item.FoundVersion = read["foundversion"].ToString();
                item.Vendor = read["vendor"].ToString();
                item.Type = read["type"].ToString();
                item.CaptureID = Int32.Parse(read["captureid"].ToString());
                item.Visible = Int32.Parse(read["visible"].ToString());
                item.Comparison = Int32.Parse(read["comparison"].ToString());

                list.Add(item);
            }

            return list;
        }


    }
}
