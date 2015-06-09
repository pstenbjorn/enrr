using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrrVa.Common
{
    public class SqlConstructors
    {

        public static string sqlBallotStyles(string tablename, string returnset)
        {
            string sqlOut = @"declare @bs  table(
	            election_name varchar(255),
	            locality_code varchar(255),
	            precinct_code varchar(255),
	            ballotdistrictids varchar(2000)
            )

            insert into @bs
            select distinct 
            election_name, locality_code, precinct_code, 
            stuff(
	            (select distinct ',' + rr.districtId
		            from enr." + tablename + @" rr
		            where rr.locality_code = r.locality_code and rr.precinct_code = rr.precinct_code
		            for xml path ('')
	            ), 1, 1, ''
            ) as d
            from enr." + tablename + @" r


            declare @ballotstyles table (
	            id int identity(1,1),
	            election_name varchar(255),
	            ballotdistrictids varchar(2000)
            )
	
	            insert into @ballotstyles 
	            select distinct election_Name, ballotdistrictids from
	            @bs

            --select * from @ballotstyles

            declare @districtballotstyle table (
	            ballot_style_id int,
	            districtId varchar(255)
            )

            insert into @districtballotstyle
            select A.id, 
	            split.a.value('.', 'varchar(255)') as Data
            from
	            (
		            SElect id, cast('<M>' + replace(ballotdistrictids, ',', '</M><M>') + '</M>' as XML) as Data
		            from @ballotstyles
	            ) as A cross apply Data.nodes('/M') as Split(a)

	
            declare @precinctballotstyles table (
	            ballot_style_id int,
	            locality_code varchar(255),
	            precinct_code varchar(255)
            )

            insert into @precinctballotstyles
            select
            bs.id, pb.locality_code, replace(pb.precinct_code, '#', '') as precinct_code
            from @bs pb
            inner join
            @ballotstyles bs on 
            pb.ballotdistrictids = bs.ballotdistrictids";

            if (returnset == "BallotStyles")
            {
                sqlOut += " select * from @ballotstyles";
            }

            if (returnset == "BallotStyleDistricts")
            {
                sqlOut += @" select distinct db.ballot_style_id, db.districtId, r.officeId from
                              @districtballotstyle db inner join
                                enr.va_results_alt r on r.districtId = db.districtId ";
            }

            if (returnset == "BallotStylePrecincts")
            {
                sqlOut += " select * from @precinctballotstyles";
            }


            return sqlOut;

        }
    }
}
