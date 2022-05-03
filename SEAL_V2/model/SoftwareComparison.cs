using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEAL_V2.db;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SEAL_V2.model
{
    class SoftwareComparison
    {
        private List<Software> a = new List<Software>();
        private List<Software> b = new List<Software>();
        private List<Software> kba = new List<Software>();
        private List<Software> kbb = new List<Software>();
        Dictionary<String, Software> aDictionary = new Dictionary<string, Software>();
        Dictionary<String, Software> bDictionary = new Dictionary<string, Software>();
        Dictionary<String, List<Software>> aDupes = new Dictionary<string, List<Software>>();
        Dictionary<String, List<Software>> bDupes = new Dictionary<string, List<Software>>();
        private List<SoftwareCompare> compareList = new List<SoftwareCompare>();

        public SoftwareComparison(List<Software> a, List<Software> b)
        {
            //a is current
            this.a = a;
            this.b = b;
        }

        public void loadDictionaries()
        {
            foreach (Software software in a)
            {
                //Check for kb number in version...
                Match kbFound = Regex.Match(software.SoftwareVersion, @"KB\d*", RegexOptions.IgnoreCase);

                if (kbFound.Success)
                {
                    kba.Add(software);
                }
                else
                {
                    //Dealing with duplicate software names... Check if software name was already added
                    if (aDictionary.ContainsKey(software.SoftwareName))
                    {
                        //Check if dupes dictionary already has a duplicate of software name
                        if (aDupes.ContainsKey(software.SoftwareName))
                        {
                            //Add if entry exists
                            aDupes[software.SoftwareName].Add(software);
                        }
                        else
                        {
                            //Create list of entry in dupes does not exist
                            List<Software> softwareList = new List<Software>();

                            //Add software to list
                            softwareList.Add(software);

                            aDupes[software.SoftwareName] = softwareList;
                        }
                    }
                    else
                    {
                        aDictionary[software.SoftwareName] = software;
                    }
                }
            }

            foreach (Software software in b)
            {
                //Check for kb number in version...
                Match kbFound = Regex.Match(software.SoftwareVersion, @"KB\d*", RegexOptions.IgnoreCase);

                if (kbFound.Success)
                {
                    kbb.Add(software);
                }
                else
                {
                    //Dealing with duplicate software names... Check if software name was already added
                    if (bDictionary.ContainsKey(software.SoftwareName))
                    {
                        //Check if dupes dictionary already has a duplicate of software name
                        if (bDupes.ContainsKey(software.SoftwareName))
                        {
                            //Add if entry exists
                            bDupes[software.SoftwareName].Add(software);
                        }
                        else
                        {
                            //Create list of entry in dupes does not exist
                            List<Software> softwareList = new List<Software>();

                            //Add software to list
                            softwareList.Add(software);

                            bDupes[software.SoftwareName] = softwareList;
                        }
                    }
                    else
                    {
                        bDictionary[software.SoftwareName] = software;
                    }
                }
            }
        }

        public void softwareComparison()
        {
            List<String> keysToDelete = new List<String>();

            foreach (KeyValuePair<String, Software> entry in aDictionary)
            {
                SoftwareCompare comparison = new SoftwareCompare();
                comparison.SoftwareName = entry.Value.SoftwareName;
                comparison.FoundVersion = entry.Value.SoftwareVersion;
                comparison.Type = entry.Value.SoftwareType;
                comparison.Vendor = entry.Value.SoftwareVendor;
                comparison.CaptureID = entry.Value.CaptureID;
                comparison.Visible = entry.Value.Visible;

                if (bDictionary.TryGetValue(entry.Value.SoftwareName, out Software foundSoftware))
                {
                    //This ensures that if previous capture b had this software object enabled as visible, then the report will show it even if in a it is NOT visible
                    comparison.Visible = foundSoftware.Visible;
                    comparison.Version = foundSoftware.SoftwareVersion;

                    //if software a is less than software b
                    if (comparator(entry.Value.SoftwareVersion, foundSoftware.SoftwareVersion) > 0)
                    {
                        comparison.Comparison = 1;
                    }
                    else if (comparator(entry.Value.SoftwareVersion, foundSoftware.SoftwareVersion) == 0)
                    {
                        comparison.Comparison = 0;
                    }
                    else
                    {
                        comparison.Comparison = 2;
                    }

                    keysToDelete.Add(entry.Key);
                }
                else
                {
                    //Software was found in A but not B so must have been added
                    comparison.Comparison = 3;
                }

                compareList.Add(comparison);
            }

            deleteDictionaryKeys(keysToDelete);

            foreach (KeyValuePair<String, Software> entry in bDictionary)
            {
                SoftwareCompare comparison = new SoftwareCompare();
                comparison.SoftwareName = entry.Value.SoftwareName;
                comparison.Version = entry.Value.SoftwareVersion;
                comparison.FoundVersion = "NOT FOUND";
                comparison.Type = entry.Value.SoftwareType;
                comparison.Vendor = entry.Value.SoftwareVendor;
                comparison.CaptureID = entry.Value.CaptureID;
                comparison.Visible = entry.Value.Visible;
                comparison.Comparison = 4;

                compareList.Add(comparison);
            }

            foreach (Software akb in kba)
            {
                Software kbToRemove = null;

                foreach (Software bkb in kbb)
                {
                    if (akb.SoftwareVersion == bkb.SoftwareVersion)
                    {
                        kbToRemove = bkb;

                        SoftwareCompare comparison = new SoftwareCompare();
                        comparison.SoftwareName = bkb.SoftwareName;
                        comparison.Version = akb.SoftwareVersion;
                        comparison.FoundVersion = bkb.SoftwareVersion;
                        comparison.Vendor = bkb.SoftwareVendor;
                        comparison.Type = bkb.SoftwareType;
                        comparison.CaptureID = bkb.CaptureID;
                        comparison.Visible = akb.Visible;
                        comparison.Comparison = 0;

                        compareList.Add(comparison);
                    }
                }

                if (kbToRemove != null)
                {
                    kbb.Remove(kbToRemove);
                }
                else
                {
                    SoftwareCompare comparison = new SoftwareCompare();
                    comparison.SoftwareName = akb.SoftwareName;
                    comparison.Version = "NOT FOUND";
                    comparison.FoundVersion = akb.SoftwareVersion;
                    comparison.Vendor = akb.SoftwareVendor;
                    comparison.Type = akb.SoftwareType;
                    comparison.CaptureID = akb.CaptureID;
                    comparison.Visible = akb.Visible;
                    comparison.Comparison = 3;

                    compareList.Add(comparison);
                }
            }

            foreach (Software bkb in kbb)
            {
                SoftwareCompare comparison = new SoftwareCompare();
                comparison.SoftwareName = bkb.SoftwareName;
                comparison.Version = bkb.SoftwareVersion;
                comparison.FoundVersion = "NOT FOUND";
                comparison.Vendor = bkb.SoftwareVendor;
                comparison.Type = bkb.SoftwareType;
                comparison.CaptureID = bkb.CaptureID;
                comparison.Visible = bkb.Visible;
                comparison.Comparison = 4;

                compareList.Add(comparison);
            }

            //Dealing with duplicates...
            foreach (KeyValuePair<String, List<Software>> entry in aDupes)
            {
                foreach (Software software in entry.Value)
                {
                    if (bDupes.TryGetValue(software.SoftwareName, out List<Software> softwareObjects))
                    {
                        int foundIndex = -1;

                        for (int i = 0; i < softwareObjects.Count; i++)
                        {
                            if (softwareObjects[i].SoftwareVersion == software.SoftwareVersion)
                            {
                                SoftwareCompare comparison = new SoftwareCompare();
                                comparison.SoftwareName = softwareObjects[i].SoftwareName;
                                comparison.Version = softwareObjects[i].SoftwareVersion;
                                comparison.FoundVersion = software.SoftwareVersion;
                                comparison.Vendor = softwareObjects[i].SoftwareVendor;
                                comparison.Type = softwareObjects[i].SoftwareType;
                                comparison.CaptureID = softwareObjects[i].CaptureID;
                                comparison.Visible = softwareObjects[i].Visible;
                                comparison.Comparison = 0;

                                compareList.Add(comparison);

                                foundIndex = i;

                                i = softwareObjects.Count;
                            }

                            if (foundIndex > -1)
                            {
                                softwareObjects.RemoveAt(foundIndex);
                            }
                        }
                    }
                    else
                    {
                        SoftwareCompare comparison = new SoftwareCompare();
                        comparison.SoftwareName = software.SoftwareName;
                        comparison.Version = "NOT FOUND";
                        comparison.FoundVersion = software.SoftwareVersion;
                        comparison.Vendor = software.SoftwareVendor;
                        comparison.Type = software.SoftwareType;
                        comparison.CaptureID = software.CaptureID;
                        comparison.Visible = software.Visible;
                        comparison.Comparison = 5;

                        compareList.Add(comparison);
                    }
                }
            }

            foreach(KeyValuePair<String, List<Software>> entry in bDupes)
            {
                foreach (Software software in entry.Value)
                {
                    SoftwareCompare comparison = new SoftwareCompare();
                    comparison.SoftwareName = software.SoftwareName;
                    comparison.Version = software.SoftwareVersion;
                    comparison.FoundVersion = "NOT FOUND";
                    comparison.Vendor = software.SoftwareVendor;
                    comparison.Type = software.SoftwareType;
                    comparison.CaptureID = software.CaptureID;
                    comparison.Visible = software.Visible;
                    comparison.Comparison = 5;

                    compareList.Add(comparison);
                }
            }
        }

        public List<SoftwareCompare> getCompareList()
        {
            return compareList;
        }

        private void deleteDictionaryKeys(List<String> keyList)
        {
            foreach (String key in keyList)
            {
                bDictionary.Remove(key);
            }
        }


        //Simple comparator for version numbers...
        private int comparator(String stringA, String stringB)
        {
            //Split version by period
            String[] stringAArray = stringA.Split('.');
            String[] stringBArray = stringB.Split('.');

            int comparison = 0;

            for (int i = 0; i < stringAArray.Length; i++)
            {
                //Check if string is integer
                if (Int32.TryParse(stringAArray[i], out int a) && Int32.TryParse(stringBArray[i], out int b))
                {
                    if (a.CompareTo(b) != 0)
                    {
                        return a.CompareTo(b);
                    }
                }
                //If just string, attempt to compare the strings
                else if (stringAArray[i].CompareTo(stringBArray[i]) != 0)
                {
                    //Console.WriteLine("STRING A: " + stringAArray[i] + " STRING B: " + stringBArray[i] + " COMPARATOR: " + stringAArray[i].CompareTo(stringBArray[i])); USED FOR TESTING

                    return stringAArray[i].CompareTo(stringBArray[i]);
                }
            }
            return comparison;
        }

    }
}
