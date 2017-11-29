// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

#if IS_DESKTOP
using System.Security.Cryptography.Pkcs;
#endif

namespace NuGet.Packaging.Signing
{
    /// <summary>
    /// Package signature information.
    /// </summary>
    public class Signature
    {
        //TODO: Clean this class

#if IS_DESKTOP

        /// <summary>
        /// A SignedCms object holding the signature and SignerInfo.
        /// </summary>
        public SignedCms SignedCms { get; }

        /// <summary>
        /// Indicates if this is an author or repository signature.
        /// </summary>
        public SignatureType Type { get; }

        /// <summary>
        /// Signature manifest containing the hash of the content manifest.
        /// </summary>
        public SignatureManifest SignatureManifest { get; }

        /// <summary>
        /// Certificate collection used as an additional certificate store when buildind chain
        /// </summary>
        public X509Certificate2Collection Certificates => SignedCms.Certificates;

        /// <summary>
        /// SignerInfo for this signature.
        /// </summary>
        public SignerInfo SignerInfo => SignedCms.SignerInfos[0];

        private Signature(SignedCms signedCms)
        {
            SignedCms = signedCms ?? throw new ArgumentNullException(nameof(signedCms));
            SignatureManifest = SignatureManifest.Load(SignedCms.ContentInfo.Content);
            Type = GetSignatureType(SignerInfo);
        }

        /// <summary>
        /// Save the signed cms signature to a stream.
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            using (var ms = new MemoryStream(SignedCms.Encode()))
            {
                ms.CopyTo(stream);
            }
        }

        /// <summary>
        /// Retrieve the bytes of the signed cms signature.
        /// </summary>
        public byte[] GetBytes()
        {
            return SignedCms.Encode();
        }

        public static Signature Load(SignedCms cms)
        {
            var signerInfoCollection = cms.SignerInfos;
            if (signerInfoCollection.Count != 1)
            {
                throw new InvalidOperationException("SignedCms has more than one signer");
            }

            return new Signature(cms);
        }

        public static Signature Load(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var cms = new SignedCms();
            cms.Decode(data);

            return Load(cms);
        }

        public static Signature Load(Stream stream)
        {
            using (stream)
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Load(ms.ToArray());
            }
        }

        // TODO Use the new attributes that justin is adding.
        private static SignatureType GetSignatureType(SignerInfo signer)
        {
            var certificate = signer.Certificate;
            if (SigningUtility.CertificateContainsEku(certificate, SigningSpecifications.V1.AuthorKeyUsageOID))
            {
               return SignatureType.Author;
            }
            else if (SigningUtility.CertificateContainsEku(certificate, SigningSpecifications.V1.RepositoryKeyUsageOID))
            {
               return SignatureType.Repository;
            }
            return SignatureType.Unknown;
        }

#else

        public byte[] GetBytes()
        {
            throw new NotSupportedException();
        }
#endif
    }
}