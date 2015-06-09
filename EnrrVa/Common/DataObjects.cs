using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace EnrrVa.Common
{
    public static class DataObjects
    {

        public static Dictionary<string, string> Localites = new Dictionary<string, string>();
        public static Election this_election = new Election("", "", "");
        public static List<Precinct> Precincts = new List<Precinct>();

        public static List<Candidate> Candidates = new List<Candidate>();
        public static List<Office> Offices = new List<Office>();
        public static List<District> Districts = new List<District>();
        public static List<Referendum> Referendums = new List<Referendum>();
        public static List<PrecinctDistrict> PrecinctDistricts = new List<PrecinctDistrict>();
        public static List<Party> Parties = new List<Party>();
        public static List<OfficePrecinct> OfficePrecincts = new List<OfficePrecinct>();
        public static List<BallotStyle> BallotStyles = new List<BallotStyle>();
        public static List<BallotStyleDistrict> BallotStyleDistricts = new List<BallotStyleDistrict>();
        public static List<BallotStylePrecinct> BallotStylePrecincts = new List<BallotStylePrecinct>();
        public static List<CandidateContest> CandidateContests = new List<CandidateContest>();
        public static List<CandidateObject> CandidateObjects = new List<CandidateObject>();
        
        public static void SetElectionData(string tablename)
        {
            //election
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct election_name, left(election_date, 10) as election_date, 
                                    election_type from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //this_election.election_name = reader["election_name"].ToString();
                    this_election.election_name = "Virginia " + reader["election_type"].ToString() + " Election " + reader["election_date"].ToString();
                    this_election.election_date = reader["election_date"].ToString();
                    this_election.election_type = reader["election_type"].ToString();
                }

            }
            //localities

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select distinct locality_code, locality_name from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Localites[reader["locality_code"].ToString()] = reader["locality_name"].ToString();
                }
            }

            //precincts 

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct replace(precinct_code, '#', '') as precinct_code, precinct_name, 
                                    locality_code, locality_name from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Precincts.Add(new Precinct(reader["precinct_name"].ToString(),
                        reader["precinct_code"].ToString(), reader["locality_name"].ToString(),
                        reader["locality_code"].ToString()));
                }
            }

            //candidates

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct candidate_name, last_name, candidateId, 
                                    case when party = '' then 'par_none' else 'par_' + left(lower(party), 3) end as party, 
                                    candidate_ballot_order, 
                                    officeId, total_votes, locality_code, replace(precinct_code, '#', '') as 
                                    precinct_code from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Candidates.Add(new Candidate(reader["candidate_name"].ToString(), reader["last_name"].ToString(),
                        reader["candidateId"].ToString(), reader["party"].ToString(), reader["candidate_ballot_order"].ToString(),
                        reader["officeId"].ToString(), reader["total_votes"].ToString(), reader["locality_code"].ToString(),
                        reader["precinct_code"].ToString()));
                }
            }

            //candidate objects
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct candidate_name, last_name, candidateId, 
                                    case when party = '' then 'par_none' else 'par_' + left(lower(party), 3) end as party, 
                                    candidate_ballot_order, 
                                    officeId from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CandidateObjects.Add(new CandidateObject(reader["candidate_name"].ToString(), reader["last_name"].ToString(),
                        reader["candidateId"].ToString(), reader["party"].ToString(), reader["candidate_ballot_order"].ToString(),
                        reader["officeId"].ToString()));
                }
            }

            //offices
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct 
                                case when officeId = '1737558668' then
	                                'Clerk of Court YORK COUNTY/POQUOSON CITY'
	                                else
                                office_name + ' ' + DESCRIPTION end as office_name, officeId, 
                                   districtId, office_ballot_order, 
								   case when election_name like '%Democrat%'
								   then 'par_dem'
								   when election_name like '%Republican%'
								   then 'par_rep'
								   else ''
								   end as party
								    from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Offices.Add(new Office(reader["office_name"].ToString(), reader["officeId"].ToString(), reader["districtId"].ToString(),
                        reader["office_ballot_order"].ToString(), reader["party"].ToString()));
                }
            }

            //districts
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct districtId, DESCRIPTION, district_type from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Districts.Add(new District(reader["districtId"].ToString(), reader["DESCRIPTION"].ToString(),
                        reader["district_type"].ToString()));
                }
            }
            //referendums
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct referendum_title, referendumId, districtId, office_ballot_order, 
                                    total_votes, negative_votes, locality_code, precinct_code 
                                    from enr." + tablename + " where referendumId <> ''";
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Referendums.Add(new Referendum(reader["referendum_title"].ToString(), reader["referendumId"].ToString(),
                        reader["districtId"].ToString(), reader["office_ballot_order"].ToString(), reader["total_votes"].ToString(),
                        reader["negative_votes"].ToString(), reader["locality_code"].ToString(), reader["precinct_code"].ToString()));
                }
            }

            //precinctdistricts
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct replace(precinct_code, '#', '') as precinct_code, 
                                    locality_code, districtid from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    PrecinctDistricts.Add(new PrecinctDistrict(reader["precinct_code"].ToString(),
                        reader["locality_code"].ToString(), reader["districtid"].ToString()));
                }
            }

            //parties
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct case when party = '' then 'none' else lower(party) end as party_name,
                                    case when party = '' then 'par_none' else 'par_' + left(lower(party), 3) end as party_id 
                                    from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Parties.Add(new Party(reader["party_name"].ToString(), reader["party_id"].ToString()));
                }
            }

            //officeprecincts
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = @"select distinct officeId, locality_code, replace(precinct_code, '#', '') as precinct_code 
                                    from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    OfficePrecincts.Add(new OfficePrecinct(reader["officeId"].ToString(), reader["locality_code"].ToString(),
                                        reader["precinct_code"].ToString()));
                }
            }

            //ballotstyles
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = SqlConstructors.sqlBallotStyles(tablename, "BallotStyles");
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BallotStyles.Add(new BallotStyle(reader["id"].ToString(), reader["election_name"].ToString(),
                                 reader["ballotdistrictids"].ToString()));
                }

            }

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = SqlConstructors.sqlBallotStyles(tablename, "BallotStyleDistricts");
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BallotStyleDistricts.Add(new BallotStyleDistrict(reader["ballot_style_id"].ToString(), 
                        reader["districtId"].ToString(), reader["officeId"].ToString()));
                }
            }

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = SqlConstructors.sqlBallotStyles(tablename, "BallotStylePrecincts");
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BallotStylePrecincts.Add(new BallotStylePrecinct(reader["ballot_style_id"].ToString(),
                        reader["locality_code"].ToString(), reader["precinct_code"].ToString()));
                }
            }

            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "select distinct candidateId, officeId, districtId from enr." + tablename;
                cmd.Connection = DataConnection.GetOpenDataConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CandidateContests.Add(new CandidateContest(reader["candidateId"].ToString(), 
                        reader["officeId"].ToString(), reader["districtId"].ToString()));
                }
            }

        }

    }

    public class Election
    {
        public string election_name { get; set; }
        public string election_date { get; set; }
        public string election_type { get; set; }
        public Election(string Election_name, string Election_date, string Election_type)
        {
            election_date = Election_name;
            election_date = Election_date;
            election_type = Election_type;
        }
    }

    public class Precinct
    {
        public string precinct_name { get; set; }
        public string precinct_code { get; set; }
        public string locality_name { get; set; }
        public string locality_code { get; set; }
        public Precinct(string PrecinctName, string PrecinctCode, string LocalityName, string LocalityCode)
        {
            precinct_name = PrecinctName;
            precinct_code = PrecinctCode;
            locality_name = LocalityName;
            locality_code = LocalityCode;
        }

    }

    public class PrecinctDistrict
    {
        public string precinct_code { get; set; }
        public string locality_code { get; set; }
        public string district_id { get; set; }
        public PrecinctDistrict (string PrecinctCode, string LocalityCode, string DistrictId)
        {
            precinct_code = PrecinctCode;
            locality_code = LocalityCode;
            district_id = DistrictId;
        }
    }

    public class BallotStyle
    {
        public string ballot_style_id { get; set; }
        public string election_name { get; set; }
        public string ballotdistrictids { get; set; }
        public BallotStyle(string BsId, string ElectionName, string Districts)
        {
            ballot_style_id = BsId;
            election_name = ElectionName;
            ballotdistrictids = Districts;
        }
    }

    public class BallotStyleDistrict
    {
        public string ballot_style_id { get; set; }
        public string district_id { get; set; }
        public string officeId { get; set; }
        public BallotStyleDistrict(string BsId, string DistrictId, string OfficeId)
        {
            ballot_style_id = BsId;
            district_id = DistrictId;
            officeId = OfficeId;
        }
    }

    public class BallotStylePrecinct
    {
        public string ballot_style_id { get; set; }
        public string locality_code { get; set; }
        public string precinct_code { get; set; }
        public BallotStylePrecinct(string BsId, string LocalityCode, string PrecinctCode)
        {
            ballot_style_id = BsId;
            locality_code = LocalityCode;
            precinct_code = PrecinctCode;
        }
    }

    public class CandidateContest
    {
        public string candidateId { get; set; }
        public string officeId { get; set; }
        public string districtId { get; set; }
        public CandidateContest(string CandidateId, string OfficeId, string DistrictId)
        {
            candidateId = CandidateId;
            officeId = OfficeId;
            districtId = DistrictId;
        }
    }

    public class Candidate
    {
        public string candidate_name { get; set; }
        public string last_name { get; set; }
        public string candidateId { get; set; }
        public string party { get; set; }
        public string candidate_ballot_order { get; set; }
        public string officeId { get; set; }
        public string total_votes { get; set; }
        public string locality_code { get; set; }
        public string precinct_code { get; set; }
        public Candidate(string CandidateName, string LastName, string CandidateId, string Party,
            string CandiateOrder, string OfficeId, string TotalVotes, string LocalityCode,
            string PrecinctCode)
        {
            candidate_name = CandidateName;
            last_name = LastName;
            candidateId = CandidateId;
            party = Party;
            candidate_ballot_order = CandiateOrder;
            officeId = OfficeId;
            total_votes = TotalVotes;
            locality_code = LocalityCode;
            precinct_code = PrecinctCode;
        }
    }

    public class CandidateObject
    {
        public string candidate_name { get; set; }
        public string last_name { get; set; }
        public string candidateId { get; set; }
        public string party { get; set; }
        public string candidate_ballot_order { get; set; }
        public string officeId { get; set; }
       public CandidateObject(string CandidateName, string LastName, string CandidateId, string Party,
            string CandiateOrder, string OfficeId)
        {
            candidate_name = CandidateName;
            last_name = LastName;
            candidateId = CandidateId;
            party = Party;
            candidate_ballot_order = CandiateOrder;
            officeId = OfficeId;
            
        }
    }

    public class Party
    {
        public string party_name { get; set; }
        public string party_id { get; set; }
        public Party(string PartyName, string PartyId)
        {
            party_name = PartyName;
            party_id = PartyId;
        }
    }

    public class Office
    {
        public string office_name { get; set; }
        public string officeId { get; set; }
        public string districtId { get; set; }
        public string office_ballot_order { get; set; }
        public string party_id { get; set; }
        public Office(string OfficeName, string OfficeId, string DistrictId, string OfficeOrder, string Party)
        {
            office_name = OfficeName;
            officeId = OfficeId;
            districtId = DistrictId;
            office_ballot_order = OfficeOrder;
            party_id = Party;
        }
    }

    public class OfficePrecinct
    {
        public string officeId { get; set; }
        public string locality_code { get; set; }
        public string precinct_code { get; set; }
        public OfficePrecinct(string OfficeId, string LocalityCode, string PrecinctCode)
        {
            officeId = OfficeId;
            locality_code = LocalityCode;
            precinct_code = PrecinctCode;
        }
    }

    public class District
    {
        public string districtId { get; set; }
        public string description { get; set; }
        public string district_type { get; set; }
        public District(string DistrictId, string Description, string DistrictType)
        {
            districtId = DistrictId;
            description = Description;
            district_type = DistrictType;
        }
    }

    public class Referendum
    {
        public string referendum_title { get; set; }
        public string referendumId { get; set; }
        public string districtId { get; set; }
        public string office_ballot_order { get; set; }
        public string total_votes { get; set; }
        public string negative_votes { get; set; }
        public string locality_code { get; set; }
        public string precinct_code { get; set; }
        public Referendum(string Title, string ReferendumId, string DistrictId, string BallotOrder,
            string TotalVotes, string NegativeVotes, string LocalityCode, string PrecinctCode)
        {
            referendum_title = Title;
            referendumId = ReferendumId;
            districtId = DistrictId;
            office_ballot_order = BallotOrder;
            total_votes = TotalVotes;
            negative_votes = NegativeVotes;
            locality_code = LocalityCode;
            precinct_code = PrecinctCode;
        }
    }
}
