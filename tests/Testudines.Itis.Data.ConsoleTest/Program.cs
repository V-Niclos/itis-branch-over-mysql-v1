using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Query.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Testudines.Data.Itis.Context;
using Testudines.Data.Itis.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Testudines.Itis.Data.ConsoleTest;
internal partial class Program
{
    static async Task Main(string[] args)
    {
        //- Declare test values
        const string _TestConnectionString = "Server=127.0.0.1;Port=3306;Database=testudines_itis;Uid=your_user_id;Pwd=your_password;";
        int[] _TestTsn = [551880, 173747, 173760,0];
        string _TestSep = new string('-', 40)+Environment.NewLine;
        Console.WriteLine("App: Testudines.Itis.Data.ConsoleTest" + Environment.NewLine + "Use for testing Testudines.Data.Itis");
        Console.WriteLine(_TestSep);


        try
        {
            var options = new DbContextOptionsBuilder<ItisDbContext>()
               .UseMySql(_TestConnectionString, ServerVersion.AutoDetect(_TestConnectionString))
               .Options;

            await using ItisDbContext db = new ItisDbContext(options);
            foreach (var tsn in _TestTsn)
            {
                await Test_TaxonomicUnit(tsn, db);
                Console.Write(_TestSep);
            }


        }
        catch (Exception xcp)
        {
            Console.WriteLine(xcp.Message);
        }
        
        

        

        Console.Write("Press any key to close ");
        Console.Write(_TestSep);
        Console.ReadKey();

      
}
    //---------------------------------------------------------
    //---------------------------------------------------------
    public static async Task Test_TaxonomicUnit(int tsn, ItisDbContext db)
    {
        var unit = await db.taxonomic_units.FindAsync(tsn);
        if (unit != null)
        {
            Console.WriteLine($" tsn={tsn} has Complete name={unit.complete_name}");
        }
        else
        {
            Console.WriteLine($" tsn={tsn} has Complete name= not found, remember tsn=0 is for test forced fail");
        }

    }
    //---------------------------------------------------------
    //---------------------------------------------------------

}
