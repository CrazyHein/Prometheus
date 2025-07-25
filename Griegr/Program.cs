using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static AMEC.PCSoftware.Crypto.CrazyHein.Orbment.OrbmentAuthorization;

namespace AMEC.PCSoftware.Crypto.CrazyHein.Orbment
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string privateKey, publicKey;
            byte[]? serialNumber;
            PhysicalAddress? phy0, phy1;
            AuthorizationCode code = AuthorizationCode.NONE;

            try
            {
                privateKey = System.IO.File.ReadAllText("./keys/private.xml");
                publicKey = System.IO.File.ReadAllText("./keys/public.xml");

                if (System.IO.File.Exists("./keys/public_tips.json") == false)
                {
                    var (m, e) = ExportPublicKey(publicKey);
                    var disturb = ASCIIEncoding.ASCII.GetBytes("AMEC Next Generation Control System");
                    using (var stream = File.Create("./keys/public_tips.json"))
                    using (var writer = new Utf8JsonWriter(stream))
                    {
                        writer.WriteStartObject();

                        writer.WriteNumber("ModulusLength", m.Length);
                        writer.WritePropertyName("ModulusArray");
                        writer.WriteStartArray();
                        for (int i = 0; i < m.Length; ++i)
                            writer.WriteNumberValue(m[i] ^ disturb[i % disturb.Length]);
                        writer.WriteEndArray();

                        writer.WriteNumber("ExponentLength", e.Length);
                        writer.WritePropertyName("ExponentArray");
                        writer.WriteStartArray();
                        for (int i = 0; i < e.Length; ++i)
                            writer.WriteNumberValue(e[i] ^ disturb[i % disturb.Length]);
                        writer.WriteEndArray();

                        writer.WriteEndObject();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("An error occurred while reading the keys:\n\t" + ex.Message);
                return;
            }

            try
            {
                if (args.Length < 3)
                    throw new ArgumentException("Expected 3 arguments at least: <serial number> <ch1 physical address> <ch2 physical address> [function]...");
                
                serialNumber = ASCIIEncoding.ASCII.GetBytes(args[0]);
                if(serialNumber.Length != 16)
                    throw new ArgumentException("Serial number must be 16 characters long.");

                phy0 = PhysicalAddress.Parse(args[1]);
                if(phy0.GetAddressBytes().Length != 6)
                    throw new ArgumentException("Physical address must be 12 characters long.");

                phy1 = PhysicalAddress.Parse(args[2]);
                if (phy1.GetAddressBytes().Length != 6)
                    throw new ArgumentException("Physical address must be 12 characters long.");


                for(int i = 3; i < args.Length; ++i)
                {
                    if (Enum.TryParse<AuthorizationCode>(args[i], true, out var parsedCode))
                        code |= parsedCode;
                    else
                        throw new ArgumentException($"Unknown function: {args[i]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading the args:\n\t" + ex.Message);
                return;
            }

            try
            {
                var lic = OrbmentAuthorization.GenerateLicence(serialNumber, phy0, phy1, code, privateKey);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Orbment Licence File|*.bin",
                    Title = "Save Orbment Licence File",
                    FileName = "lic",
                    DefaultExt = "bin",
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    System.IO.File.WriteAllBytes(saveFileDialog.FileName, lic.ToByteArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while generating the licence:\n\t" + ex.Message);
                return;
            }
        }
    }
}
