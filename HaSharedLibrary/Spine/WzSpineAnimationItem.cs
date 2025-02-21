﻿/*
 * Copyright (c) 2018~2021, LastBattle https://github.com/lastbattle
 * Copyright (c) 2010~2013, haha01haha http://forum.ragezone.com/f701/release-universal-harepacker-version-892005/

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using MapleLib;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using Microsoft.Xna.Framework.Graphics;
using spine_2._1._25_netcore;

namespace HaSharedLibrary.Spine {
	public class WzSpineAnimationItem {
		// Spine 

		/// <summary>
		///  Whether the renderer will assume that colors have premultiplied alpha. 
		///  
		///  A variation of a bitmap image or alpha blending calculation in which the RGB color values are assumed 
		///  to be already multiplied by an alpha channel, to reduce computations during Alpha blending; 
		///  uses the blend operation: dst *= (1 - alpha) + src; capable of mixing alpha blending with additive blending effects
		/// </summary>
		public bool PremultipliedAlpha { get; set; }

		public SkeletonData SkeletonData { get; private set; }

		// pre-loading
		private readonly WzStringProperty wzSpineAtlasPropertyNode;

		/// <summary>
		/// SpineAnimationItem Constructor
		/// </summary>
		/// <param name="wzSpineAtlasPropertyNode">.atlas WzStringProperty</param>
		/// <param name="loadWithJson"></param>
		public WzSpineAnimationItem(WzStringProperty wzSpineAtlasPropertyNode) {
			this.wzSpineAtlasPropertyNode = wzSpineAtlasPropertyNode;
		}

		/// <summary>
		/// Load spine resources 
		/// </summary>
		/// <param name="graphicsDevice"></param>
		public void LoadResources(GraphicsDevice graphicsDevice) {
			var textureLoader = new WzSpineTextureLoader(wzSpineAtlasPropertyNode.Parent, graphicsDevice);

			var skeletonData = WzSpineAtlasLoader.LoadSkeleton(wzSpineAtlasPropertyNode, textureLoader);
			if (skeletonData == null) return;

			bool pma;
			if (wzSpineAtlasPropertyNode.Parent is WzImageProperty imgProperty) {
				pma = imgProperty["PMA"].ReadValue(0) > 0;
			} else {
				pma = ((WzImage) wzSpineAtlasPropertyNode.Parent)["PMA"].ReadValue(0) > 0;
			}

			SkeletonData = skeletonData;
			PremultipliedAlpha =
				pma; //  whether the renderer will assume that colors have premultiplied alpha. Default is true.
		}
	}
}