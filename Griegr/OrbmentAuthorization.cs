using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml;

namespace AMEC.PCSoftware.Crypto.CrazyHein.Orbment
{
    public class OrbmentAuthorization
    {
        public class Licence
        {
            public ReadOnlyMemory<byte> Encrypted { get; }

            public Licence(ReadOnlySpan<byte> encrypted) 
            {
                if (encrypted.Length != 256)
                    throw new ArgumentException("Encrypted data must be 256 bytes.");

                var data = new byte[encrypted.Length];
                encrypted.CopyTo(data);
                Encrypted = data.AsMemory();
            }

            public byte[] ToByteArray()
            {
                byte[] ret = new byte[4 + 256];
                BitConverter.GetBytes(Encrypted.Length).CopyTo(ret, 0);
                Encrypted.CopyTo(ret.AsMemory().Slice(4));
                return ret;
            }

            public static Licence FromByteArray(ReadOnlySpan<byte> data)
            {
                if (data.Length != 4 + 256)
                    throw new ArgumentException("Byte array must be 260 bytes (4 bytes for length + 256 bytes for encrypted data).");

                int encryptedLength = BitConverter.ToInt32(data.Slice(4 + 256 + 4 + 256, 4));
                if (encryptedLength != 256)
                    throw new ArgumentException("Encrypted data must be 256 bytes.");

                ReadOnlySpan<byte> encrypted = data.Slice(4, encryptedLength);
                return new Licence(encrypted);
            }
        }

        public class Authorization
        {
            public ReadOnlyMemory<byte> SerialNo { get; }
            public ReadOnlyMemory<byte> PHY_CH1 { get; }
            public ReadOnlyMemory<byte> PHY_CH2 { get; }
            public AuthorizationCode Code { get; }

            public Authorization(ReadOnlySpan<byte> serialNo, PhysicalAddress phy_ch1, PhysicalAddress phy_ch2, AuthorizationCode code)
            {
                if (serialNo.Length != 16)
                {
                    throw new ArgumentException("The serial number should be a 16-byte array.");
                }

                var data = new byte[serialNo.Length];
                serialNo.CopyTo(data);
                SerialNo = data.AsMemory();

                PHY_CH1 = phy_ch1.GetAddressBytes().AsMemory();
                PHY_CH2 = phy_ch2.GetAddressBytes().AsMemory();

                Code = code;
            }

            public byte[] ToByteArray()
            {
                byte[] ret = new byte[16 + 6 + 6 + 4];
                SerialNo.CopyTo(ret.AsMemory().Slice(0));
                PHY_CH1.CopyTo(ret.AsMemory().Slice(16));
                PHY_CH2.CopyTo(ret.AsMemory().Slice(16 + 6));
                BitConverter.GetBytes((uint)Code).CopyTo(ret, 16 + 6 + 6);
                return ret;
            }
        }

        [Flags]
        public enum AuthorizationCode : uint
        {
            NONE    = 0x00000000,
            DAQ     = 0x00000001,
            RCP  = 0x00000002,
        }

        public static void GenerateRSASecretKey(out string privateKey, out string publicKey)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                privateKey = rsa.ToXmlString(true);
                publicKey = rsa.ToXmlString(false);
            }
        }

        public static (byte[] m, byte[] e) ExportPublicKey(string publicKey)
        {
            XmlDocument publicKeyDocument = new XmlDocument();
            publicKeyDocument.LoadXml(publicKey);
            string modulus = publicKeyDocument.SelectSingleNode("//Modulus").InnerText;
            string exponent = publicKeyDocument.SelectSingleNode("//Exponent").InnerText;
            return (Convert.FromBase64String(modulus), Convert.FromBase64String(exponent));
        }

        public static Licence GenerateLicence(ReadOnlySpan<byte> serialNo, PhysicalAddress phy0, PhysicalAddress phy1, AuthorizationCode code, string privateKey)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                rsa.FromXmlString(privateKey);

                //var publicKeyByteArray = ExportPublicKey(publicKey);


                //对象标识符(OBJECT IDENTIFIER, OID) 的编码规则
                //MD2:      30 20 30 0c 06 08 2a 86 48 86 f7 0d 02 02 05 00 04 10
                //MD5:      30 20 30 0c 06 08 2a 86 48 86 f7 0d 02 05 05 00 04 10
                //SHA-1:    30 21 30 09 06 05 2b 0e 03 02 1a 05 00 04 14
                //SHA-256:  30 31 30 0d 06 09 60 86 48 01 65 03 04 02 01 05 00 04 20
                //SHA-384:  30 41 30 0d 06 09 60 86 48 01 65 03 04 02 02 05 00 04 30
                //SHA-512:  30 51 30 0d 06 09 60 86 48 01 65 03 04 02 03 05 00 04 40


                Authorization authorization = new Authorization(serialNo, phy0, phy1, code);
                byte[] encrypted = rsa.SignHash(authorization.ToByteArray(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                return new Licence(encrypted);
            }
        }

        public static bool VerifyLicence(Licence licence, ReadOnlySpan<byte> serialNo, PhysicalAddress phy0, PhysicalAddress phy1, AuthorizationCode code, string publicKey)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                rsa.FromXmlString(publicKey);

                Authorization authorization = new Authorization(serialNo, phy0, phy1, code);
                return rsa.VerifyHash(authorization.ToByteArray(), licence.Encrypted.Span, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
    }
}
