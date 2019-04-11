using System;
using System.Linq;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using System.IO;

namespace DataCollector
{
    internal class Encryption
    {
        private PgpEncKeys mPgpKeys;
        private const int BufferSize = 0x10000;

        public Encryption(PgpEncKeys encKeys)
        {
            mPgpKeys = encKeys;
        }

        public void EncryptSign(Stream oStream, FileInfo plainFileInfo)
        {
            using (Stream encryptedO = ChEncryptedO(oStream))
            using (Stream compressedO = ChCompressedO(encryptedO))
            {
                PgpSignatureGenerator sigGen = InitSigGen(compressedO);
                using (Stream literalO = ChLiteralO(compressedO, plainFileInfo))
                using (FileStream iFile = plainFileInfo.OpenRead())
                {
                    OutputSign(compressedO, literalO, iFile, sigGen);
                }
            }
        }

        private static void OutputSign(Stream compressedO, Stream literalO, FileStream iFile, PgpSignatureGenerator sigGen)
        {
            int length = 0;
            byte[] buf = new byte[BufferSize];
            while ((length = iFile.Read(buf, 0, buf.Length)) > 0)
            {
                literalO.Write(buf, 0, length);
                sigGen.Update(buf, 0, length);
            }
            sigGen.Generate().Encode(compressedO);
        }

        private Stream ChEncryptedO(Stream oStream)
        {
            PgpEncryptedDataGenerator encryptedDataGen;
            encryptedDataGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, new SecureRandom());
            encryptedDataGen.AddMethod(mPgpKeys.PublicKey);
            return encryptedDataGen.Open(oStream, new byte[BufferSize]);
        }

        private Stream ChCompressedO(Stream encryptedO)
        {
            PgpCompressedDataGenerator compressedDataGen = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
            return compressedDataGen.Open(encryptedO);
        }

        private static Stream ChLiteralO(Stream compressedO, FileInfo plainFileInfo)
        {
            PgpLiteralDataGenerator pgpLiteralDataGen = new PgpLiteralDataGenerator();
            return pgpLiteralDataGen.Open(compressedO, PgpLiteralData.Binary, plainFileInfo);
        }

        private PgpSignatureGenerator InitSigGen(Stream compressedO)
        {
            const bool Crit = false;
            const bool Nestd = false;
            PublicKeyAlgorithmTag tag = mPgpKeys.SecretKey.PublicKey.Algorithm;
            PgpSignatureGenerator pgpSigGen = new PgpSignatureGenerator(tag, HashAlgorithmTag.Sha1);
            pgpSigGen.InitSign(PgpSignature.BinaryDocument, mPgpKeys.PrivateKey);
            foreach (string userId in mPgpKeys.SecretKey.PublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator subpktGen = new PgpSignatureSubpacketGenerator();
                subpktGen.SetSignerUserId(Crit, userId);
                pgpSigGen.SetHashedSubpackets(subpktGen.Generate());
                break;
            }
            pgpSigGen.GenerateOnePassVersion(Nestd).Encode(compressedO);
            return pgpSigGen;
        }
    }

    public class PgpEncKeys
    {
        public PgpPublicKey PublicKey { get; private set; }
        public PgpPrivateKey PrivateKey { get; private set; }
        public PgpSecretKey SecretKey { get; private set; }

        public PgpEncKeys(string publicKeyp, string privateKeyp, string password)
        {
            PublicKey = ReadPublicKey(publicKeyp);
            SecretKey = ReadSecretKey(privateKeyp);
            PrivateKey = ReadPrivateKey(password);
        }

        private PgpPublicKey ReadPublicKey(string publicKeyp)
        {
            using (Stream keyIn = File.OpenRead(publicKeyp))
            using (Stream iStream = PgpUtilities.GetDecoderStream(keyIn))
            {
                PgpPublicKeyRingBundle publicKeyRingBundle = new PgpPublicKeyRingBundle(iStream);
                PgpPublicKey fKey = GetFirstPublicKey(publicKeyRingBundle);
                if (fKey != null)
                {
                    return fKey;
                }
            }
            throw new ArgumentException("No PGP Key Found");
        }

        private PgpPublicKey GetFirstPublicKey(PgpPublicKeyRingBundle publicKeyRingBundle)
        {
            foreach (PgpPublicKeyRing keyRing in publicKeyRingBundle.GetKeyRings())
            {
                PgpPublicKey key = keyRing.GetPublicKeys().Cast<PgpPublicKey>().Where(i => i.IsEncryptionKey).FirstOrDefault();
                if (key != null)
                {
                    return key;
                }
            }
            return null;
        }

        private PgpSecretKey ReadSecretKey(string privateKeyp)
        {
            using (Stream keyIn = File.OpenRead(privateKeyp))
            using (Stream iStream = PgpUtilities.GetDecoderStream(keyIn))
            {
                PgpSecretKeyRingBundle secretKeyRingBundle = new PgpSecretKeyRingBundle(iStream);
                PgpSecretKey fKey = GetFirstSecretKey(secretKeyRingBundle);
                if (fKey != null)
                {
                    return fKey;
                }
            }
            throw new ArgumentException("No PGP Key Found");
        }

        private PgpSecretKey GetFirstSecretKey(PgpSecretKeyRingBundle secretKeyRingBundle)
        {
            foreach (PgpSecretKeyRing keyRing in secretKeyRingBundle.GetKeyRings())
            {
                PgpSecretKey key = keyRing.GetSecretKeys().Cast<PgpSecretKey>().Where(i => i.IsSigningKey).FirstOrDefault();
                if (key != null)
                {
                    return key;
                }
            }
            return null;
        }

        private PgpPrivateKey ReadPrivateKey(string password)
        {
            PgpPrivateKey privateKey = SecretKey.ExtractPrivateKey(password.ToCharArray());
            if (privateKey != null)
            {
                return privateKey;
            }
            throw new ArgumentException("No PGP Key Found");
        }
    }
}
