﻿using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Depra.Assets.Runtime.Abstract.Loading;
using Depra.Assets.Runtime.Bundle.Exceptions;
using Depra.Assets.Runtime.Bundle.Strategies.Web;
using Depra.Assets.Runtime.Common;
using Depra.Coroutines.Domain.Entities;
using UnityEngine;
using UnityEngine.Networking;

namespace Depra.Assets.Runtime.Bundle.Files.Web
{
    public sealed class AssetBundleFromWeb : AssetBundleFile
    {
        private readonly ICoroutineHost _coroutineHost;

        public AssetBundleFromWeb(AssetIdent ident, ICoroutineHost coroutineHost = null) :
            base(ident, coroutineHost) { }

        protected override AssetBundle LoadOverride()
        {
            using var request = UnityWebRequestAssetBundle.GetAssetBundle(Path);
            request.SendWebRequest();

            while (request.isDone == false)
            {
                // Spinning for Synchronous Behavior (blocking).
            }

            EnsureRequestResult(request, Path, exception => throw exception);
            return DownloadHandlerAssetBundle.GetContent(request);
        }

        /// <summary>
        /// Loads asset bundle form server.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AssetBundleLoadingException"></exception>
        protected override IEnumerator LoadingProcess(IAssetLoadingCallbacks<AssetBundle> callbacks)
        {
            using var webRequest = UnityWebRequestAssetBundle.GetAssetBundle(Path);
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
            {
                callbacks.InvokeProgressEvent(webRequest.downloadProgress);
                yield return null;
            }

            EnsureRequestResult(webRequest, Path, callbacks.InvokeFailedEvent);
            var downloadedBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
            callbacks.InvokeLoadedEvent(downloadedBundle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureRequestResult(UnityWebRequest request, string uri, Action<Exception> onFailed = null)
        {
            if (request.CanGetResult() == false)
            {
                onFailed?.Invoke(new AssetBundleLoadingException(uri));
            }
        }
    }
}