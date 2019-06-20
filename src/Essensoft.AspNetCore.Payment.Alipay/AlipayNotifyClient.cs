﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Essensoft.AspNetCore.Payment.Alipay.Parser;
using Essensoft.AspNetCore.Payment.Alipay.Utility;

namespace Essensoft.AspNetCore.Payment.Alipay
{
    public class AlipayNotifyClient : IAlipayNotifyClient
    {
        #region AlipayNotifyClient Constructors

        public AlipayNotifyClient()
        {

        }

        #endregion

        #region IAlipayNotifyClient Members

        public Task<T> ExecuteAsync<T>(IDictionary<string, string> parameters, AlipayOptions options) where T : AlipayNotify
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.SignType))
            {
                throw new ArgumentNullException(nameof(options.SignType));
            }

            if (string.IsNullOrEmpty(options.AppPrivateKey))
            {
                throw new ArgumentNullException(nameof(options.AppPrivateKey));
            }

            var parser = new AlipayDictionaryParser<T>();
            var rsp = parser.Parse(parameters);
            CheckNotifySign(parameters, options);
            return Task.FromResult(rsp);
        }

        #endregion

        #region Common Method

        private void CheckNotifySign(IDictionary<string, string> dictionary, AlipayOptions options)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                throw new AlipayException("sign check fail: dictionary is Empty!");
            }

            if (!dictionary.TryGetValue("sign", out var sign))
            {
                throw new AlipayException("sign check fail: sign is Empty!");
            }

            var prestr = GetSignContent(dictionary);
            if (!AlipaySignature.RSACheckContent(prestr, sign, options.AlipayPublicKey, options.SignType))
            {
                throw new AlipayException("sign check fail: check Sign Data Fail!");
            }
        }

        private string GetSignContent(IDictionary<string, string> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            var sortPara = new SortedDictionary<string, string>(dictionary);
            var sb = new StringBuilder();
            foreach (var iter in sortPara)
            {
                if (!string.IsNullOrEmpty(iter.Value) && iter.Key != "sign" && iter.Key != "sign_type")
                {
                    sb.Append(iter.Key).Append("=").Append(iter.Value).Append("&");
                }
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        #endregion
    }
}