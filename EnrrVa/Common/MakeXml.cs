using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data.SqlClient;
using System.IO;

namespace EnrrVa.Common
{
    public class MakeXml
    {
        static public string Beautify(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

        public static XmlDocument vsscGenerate(String tablename)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            int unicode = 58;
            char character = (char)unicode;
            string tx = character.ToString();

            XmlElement ElectionReport = doc.CreateElement(string.Empty, "ElectionReport", string.Empty);
            doc.AppendChild(ElectionReport);
            ElectionReport.SetAttribute("Format", "PrecinctLevel");
            ElectionReport.SetAttribute("GeneratedDate", DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            ElectionReport.SetAttribute("Issuer", "Commonwealth of Virginia");
            ElectionReport.SetAttribute("IssuerAbbreviation", "VA");
            ElectionReport.SetAttribute("Sequence", "0");
            ElectionReport.SetAttribute("SequenceEnd", "0");
            ElectionReport.SetAttribute("Status", "UnofficialPartial");
            ElectionReport.SetAttribute("VendorApplicationId", "EIS - Generated");

            XmlElement Code = doc.CreateElement(string.Empty, "Code", string.Empty);
            ElectionReport.AppendChild(Code);
            Code.SetAttribute("Type", "Fips");
            Code.SetAttribute("Value", "51");

            //GpUnitCollection

            XmlElement GpUnitCollection = doc.CreateElement(string.Empty, "GpUnitCollection", string.Empty);
            ElectionReport.AppendChild(GpUnitCollection);

            XmlElement GpUnit = doc.CreateElement(string.Empty, "GpUnit", string.Empty);
            GpUnitCollection.AppendChild(GpUnit);
            GpUnit.SetAttribute("Name", "Virginia");
            GpUnit.SetAttribute("ObjectId", "STATE_Virginia_51");
            //GpUnit.SetAttribute("Type", "State");
            GpUnit.SetAttribute("xsi" + tx + "type", "ReportingUnit");

            List<string> Localities = new List<string>();
            Dictionary<string, string> Precincts = new Dictionary<string, string>();

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select distinct 'LOCALITY_' + locality_code as loc, locality_code from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Localities.Add(reader["locality_code"].ToString());
                    XmlElement ComposingGpUnitId = doc.CreateElement(string.Empty, "ComposingGpUnitId", string.Empty);
                    ComposingGpUnitId.InnerText = reader["loc"].ToString();
                    GpUnit.AppendChild(ComposingGpUnitId);
                }

            }

            foreach (string locality in Localities)
            {

                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "select distinct 'PRECINCT_' + locality_code + precinct_code as pre, precinct_code from enr.va_results_alt WHERE locality_code = " + locality;
                    cmd.Connection = DataConnection.GetOpenDataConnection();
                    SqlDataReader reader = cmd.ExecuteReader();
                    XmlElement GpUnitLoc = doc.CreateElement(string.Empty, "GpUnit", string.Empty);
                    GpUnitLoc.SetAttribute("ObjectId", "LOCALITY_" + locality);
                    GpUnitLoc.SetAttribute("Type", "County");
                    GpUnitCollection.AppendChild(GpUnitLoc);

                    while (reader.Read())
                    {
                        Precincts[locality] = reader["precinct_code"].ToString();
                        XmlElement ComposingGpUnitIdPre = doc.CreateElement(string.Empty, "ComposingGpUnitId", string.Empty);
                        ComposingGpUnitIdPre.InnerText = reader["pre"].ToString();
                        GpUnitLoc.AppendChild(ComposingGpUnitIdPre);
                    }

                }

            }

            return doc;
        }

        public static String ToXml(XDocument xDoc)
        {
            var sw = new Utf8StringWriter();
            xDoc.Save(sw);
            string s = (sw.GetStringBuilder().ToString());
            return s;
        }

        
        public static XDocument xGen(string tablename)
        {
            EnrrVa.Common.DataObjects.SetElectionData(tablename);
            XNamespace ns = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XAttribute prefix = new XAttribute(XNamespace.Xmlns + "xsi", ns);
            XNamespace ns2 = XNamespace.Get("http://grouper.ieee.org/groups/1622/groups/2/V1/1622_2-election_results_reporting.xsd");
            XNamespace xsi = ns;
            XNamespace schemaLocation = XNamespace.Get("http://grouper.ieee.org/groups/1622/groups/2/V1/1622_2-election_results_reporting.xsd http://grouper.ieee.org/groups/1622/groups/2/V1/1622_2-election_results_reporting.xsd");
            
            //objects
            //Construct GpReportingUnits
            

            XElement locReportingUnits = new XElement(ns2 + "GpUnit", 
                                new XAttribute("Name", "Virginia"), 
                                new XAttribute("ObjectId", "st_virginia_051"),
                                new XAttribute("Type", "State"),
                                new XAttribute(xsi + "type", "ReportingUnit")
                                );
            XElement GpReportingUnit = new XElement(ns2 + "GpUnitCollection", locReportingUnits);

            foreach (var item in DataObjects.Localites)
            {
                locReportingUnits.Add(new XElement(ns2 + "ComposingGpUnitId", "loc_" + item.Key));
            }

            foreach (var item in DataObjects.Districts)
            {
                locReportingUnits.Add(new XElement(ns2 + "ComposingGpUnitId", "dis_" + item.districtId));
            }

            foreach (var item in DataObjects.Localites)
            {
                XElement xLoca = new XElement(ns2 + "GpUnit", new XAttribute("ObjectId", "loc_" + item.Key),
                    new XAttribute("Name", item.Value),
                    new XAttribute("Type", "County"), new XAttribute(xsi + "type", "ReportingUnit")
                    );
                foreach (var pre in DataObjects.Precincts)
                {
                    if (pre.locality_code == item.Key)
                    {
                        string precode = pre.precinct_code;
                        precode = precode.Replace("#", "");
                        xLoca.Add(new XElement(ns2 + "ComposingGpUnitId", "pre_" + pre.locality_code + precode));
                    }

                }
                GpReportingUnit.Add(xLoca);
            }

            foreach (var item in DataObjects.Districts)
            {
                XElement xdis = new XElement(ns2 + "GpUnit", 
                    new XAttribute("IsElectoralDistrict", "true"),
                    new XAttribute("Name", item.district_type + " " + item.description), 
                    new XAttribute("ObjectId", "dis_" + item.districtId),
                    new XAttribute("Type", "Other"),
                    new XAttribute(xsi + "type", "ReportingUnit")
                    );
                foreach (var pd in DataObjects.PrecinctDistricts)
                {
                    if (pd.district_id == item.districtId)
                    {
                        xdis.Add(new XElement(ns2 + "ComposingGpUnitId", "pre_" + pd.locality_code + pd.precinct_code));
                    }
                }
                GpReportingUnit.Add(xdis);
            }

            //precinct counted status
            foreach (var item in DataObjects.Precincts)
            {
                XElement xpre = new XElement(ns2 + "GpUnit",
                    new XAttribute("Name", item.precinct_name),
                    new XAttribute("ObjectId", "pre_" + item.locality_code + item.precinct_code),
                    new XAttribute("Type", "Precinct"), new XAttribute(xsi+"type", "ReportinUnit")
                    );

                GpReportingUnit.Add(xpre);
            }


            //PartyCollection
            XElement PartyCollection = new XElement(ns2 + "PartyCollection");

            foreach (var item in DataObjects.Parties)
            {
                XElement xpar = new XElement(ns2 + "Party", new XAttribute("ObjectId", item.party_id),
                    new XElement(ns2 + "Name", new XElement(ns2 + "LanguageString", 
                        new XAttribute("Language", "en"),
                        item.party_name)
                    ));
                PartyCollection.Add(xpar);
            }

            //OfficeCollection
            XElement OfficeCollection = new XElement(ns2 + "OfficeCollection");

            foreach (var item in DataObjects.Offices)
            {
                XElement xoff = new XElement(ns2 + "Office", new XAttribute("ObjectId", "off_" + item.officeId),
                    new XElement(ns2 + "Name", new XElement(ns2 + "LanguageString",
                        new XAttribute("Language", "en"), item.office_name
                        )), new XElement(ns2 + "JurisdictionalScopeId", "dis_" + item.districtId)
                    );
                OfficeCollection.Add(xoff);
            }

            //Election 
            XElement Election = new XElement(
                    ns2 + "Election", 
                    new XAttribute("Date", DataObjects.this_election.election_date), 
                    new XAttribute("Type", DataObjects.this_election.election_type),
                    new XElement(ns2 + "Name", new XElement(ns2 + "LanguageString", new XAttribute("Language", "en"), 
                            DataObjects.this_election.election_name)),
                    new XElement(ns2 + "Code", new XAttribute("Type", "Fips"), new XAttribute("Value", "51")),
                    new XElement(ns2 + "ElectionScopeId", "st_virginia_051")
                    
                    );

            //BallotStyleCollection
            XElement BallotStyleCollection = new XElement(ns2 + "BallotStyleCollection");

            foreach (var bs in DataObjects.BallotStyles)
            {
                XElement BallotStyle = new XElement(ns2 + "BallotStyle");
                foreach (var dbs in DataObjects.BallotStyleDistricts)
                {
                    if (dbs.ballot_style_id == bs.ballot_style_id)
                    {
                        XElement xoc = new XElement(ns2 + "OrderedContest", 
                            new XElement(ns2 + "ContestId", "con_" + dbs.officeId)
                            );
                        foreach (var cc in DataObjects.CandidateContests)
                        {
                            if (cc.officeId == dbs.officeId)
                            {
                                XElement xcc = new XElement(ns2 + "OrderedBallotSelectionId", "balsel_" + cc.candidateId);

                                xoc.Add(xcc);
                            }
                        }


                        BallotStyle.Add(xoc);
                    }
                    
                }
                foreach (var prbs in DataObjects.BallotStylePrecincts)
                {
                    if (prbs.ballot_style_id == bs.ballot_style_id)
                    {
                        XElement xprbs = new XElement(ns2 + "GpUnitId", "pre_" + prbs.locality_code + prbs.precinct_code);
                        BallotStyle.Add(xprbs);
                    }
                    
                }

                BallotStyleCollection.Add(BallotStyle);
            }

            XElement CandidateCollection = new XElement(ns2 + "CandidateCollection");

            foreach (var cand in DataObjects.CandidateObjects)
            {
                XElement xcand = new XElement(ns2 + "Candidate",
                    new XAttribute("ObjectId", "cand_" + cand.candidateId), 
                    new XAttribute("SequenceOrder", cand.candidate_ballot_order),
                    new XElement(ns2 + "BallotName", new XElement(ns2 + "LanguageString", new XAttribute("Language", "en"), 
                        cand.candidate_name)),
                    new XElement(ns2 + "PartyId", cand.party)
                    );
                CandidateCollection.Add(xcand);
            }

            XElement ContestCollection = new XElement(ns2 + "ContestCollection");

            foreach (var cont in DataObjects.Offices)
            {
                XElement xcont = new XElement(ns2 + "Contest",
                    new XAttribute("Name", cont.office_name), new XAttribute("ObjectId", "con_" + cont.officeId),
                    new XAttribute(xsi + "type", "CandidateContest"), new XAttribute("SequenceOrder", cont.office_ballot_order),
                    new XAttribute("VotesAllowed", "1")
                    );
                XElement xbalsel = new XElement(ns2 + "BallotSelection");

                foreach (var cand in DataObjects.CandidateContests)
                {
                    if (cand.officeId == cont.officeId)
                    {
                        XElement xcand = new XElement(ns2 + "BallotSelection", new XAttribute("IsWriteIn", "false"), 
                            new XAttribute("ObjectId", "balsel_" + cand.candidateId), 
                            new XAttribute(xsi + "type", "CandidateSelection"));
                            XElement xcandres = new XElement(ns2 + "VoteCountsCollection");

                        foreach (var candres in DataObjects.Candidates)
                        {
                            if (candres.candidateId == cand.candidateId)
                            {
                                XElement xVote = new XElement(ns2 + "VoteCounts", new XAttribute("Count", candres.total_votes),
                                        new XAttribute("Type", "Total"), 
                                        new XElement(ns2 + "GpUnitId", "pre_" + candres.locality_code + candres.precinct_code)
                                    );
                                xcandres.Add(xVote);
                            }

                        }
                        xcand.Add(xcandres);
                        xcand.Add(new XElement(ns2 + "CandidateId", "cand_" + cand.candidateId));
                        xbalsel.Add(xcand);
                    }
                }
                xcont.Add(xbalsel);
                xcont.Add(new XElement(ns2 + "JurisdictionalScopeId", "dis_" + cont.districtId));
                xcont.Add(new XElement(ns2 + "OfficeId", "off_" + cont.officeId));
                xcont.Add(new XElement(ns2 + "PrimaryPartyId", cont.party_id));
                        
                ContestCollection.Add(xcont);
            }


            Election.Add(BallotStyleCollection);
            Election.Add(CandidateCollection);
            Election.Add(ContestCollection);

            XDocument doc = new XDocument(
                 new XDeclaration("1.0", "itf-8", "yes"),
                 new XElement(ns2 + "ElectionReport", prefix, new XAttribute("Format", "PrecinctLevel"), 
                     new XAttribute("GeneratedDate", DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)),
                     new XAttribute("Issuer", "Virginia"), new XAttribute("IssuerAbbreviation", "VA"),
                     new XAttribute("Sequence", "0"), new XAttribute("SequenceEnd", "0"),
                     new XAttribute("Status", "UnofficialPartial"),
                     new XAttribute("VendorApplicationId", "EIS-Enrr-VA"),
                     new XAttribute("xmlns", "http://grouper.ieee.org/groups/1622/groups/2/V1/1622_2-election_results_reporting.xsd"),
                     new XAttribute(xsi + "schemaLocation", schemaLocation),
                     
                    new XElement(ns2 + "Code", new XAttribute("Type", "Fips"), new XAttribute("Value", "51")),
                    GpReportingUnit, PartyCollection, OfficeCollection, Election
    
                )  
           );

            return doc;
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}
