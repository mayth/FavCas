using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics.Contracts;

namespace FavCas
{
    /// <summary>
    /// 認証データの読み取りと保存を行います。
    /// </summary>
    class Authentication
    {
        /// <summary>
        /// デフォルトの認証情報ファイル名
        /// </summary>
        static readonly string defaultAuthFile = "auth.dat";
        /// <summary>
        /// 認証情報ファイル名
        /// </summary>
        string AuthFilePath { get; set; }
        /// <summary>
        /// 認証情報を読み取れているか否か
        /// </summary>
        public bool IsAuthorized { get; private set; }
        /// <summary>
        /// Access Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Access Token Secret
        /// </summary>
        public string TokenSecret { get; set; }

        /// <summary>
        /// <see cref="Authentication"/>クラスを初期化します。
        /// </summary>
        /// <remarks>デフォルトの認証情報ファイルを使用します。</remarks>
        public Authentication()
            : this(defaultAuthFile) { }

        /// <summary>
        /// 指定された認証情報ファイルを使用して<see cref="Authentication"/>クラスを初期化します。
        /// </summary>
        /// <param name="authFilePath">認証情報ファイルへのパス</param>
        public Authentication(string authFilePath)
        {
            Contract.Requires<ArgumentNullException>(authFilePath != null);

            AuthFilePath = authFilePath;
            if (File.Exists(AuthFilePath))
            {
                // ファイルが存在していればXMLを読み取る。
                try
                {
                    if (!string.IsNullOrWhiteSpace(AuthFilePath))
                    {
                        XElement doc = XElement.Parse(File.ReadAllText(AuthFilePath));
                        Token = doc.Element("token").Value;
                        TokenSecret = doc.Element("tokenSecret").Value;
                        IsAuthorized = true;
                    }
                    else
                    {
                        throw new FileNotFoundException(AuthFilePath);
                    }
                }
                catch
                {
                    // 読み取り中に例外があれば認証データの読み込みに失敗。
                    IsAuthorized = false;
                }
            }
            else
            {
                // そもそもファイルがなければ未認証状態にする。
                IsAuthorized = false;
            }
        }

        /// <summary>
        /// 認証情報ファイルを保存します。
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(TokenSecret))
                return;

            if (!string.IsNullOrWhiteSpace(AuthFilePath))
            {
                using (XmlWriter writer = XmlWriter.Create(AuthFilePath))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("twAuth");
                    writer.WriteElementString("token", Token);
                    writer.WriteElementString("tokenSecret", TokenSecret);
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }
    }
}
