// Open Asset Import Library (assimp)
//
// Copyright (c) 2006-2016, assimp team, 2018, The openBVE Project
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms,
// with or without modification, are permitted provided that the
// following conditions are met:
//
// * Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// * Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// * Neither the name of the assimp team, nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission of the assimp team.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//
//
// ******************************************************************************
//
// AN EXCEPTION applies to all files in the ./test/models-nonbsd folder.
// These are 3d models for testing purposes, from various free sources
// on the internet. They are - unless otherwise stated - copyright of
// their respective creators, which may impose additional requirements
// on the use of their work. For any of these models, see
// <model-name>.source.txt for more legal information. Contact us if you
// are a copyright holder and believe that we credited you inproperly or
// if you don't want your files to appear in the repository.
//
//
// ******************************************************************************
//
// Poly2Tri Copyright (c) 2009-2010, Poly2Tri Contributors
// http://code.google.com/p/poly2tri/
//
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// * Neither the name of Poly2Tri nor the names of its contributors may be
//   used to endorse or promote products derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace AssimpNET.Obj
{
	// ---------------------------------------------------------------------------
	/** @brief Enumerates the types of geometric primitives supported by Assimp.
	 *
	 *  @see aiFace Face data structure
	 *  @see aiProcess_SortByPType Per-primitive sorting of meshes
	 *  @see aiProcess_Triangulate Automatic triangulation
	 *  @see AI_CONFIG_PP_SBP_REMOVE Removal of specific primitive types.
	 */
	public enum PrimitiveType
	{
		/** A point primitive.
		 *
		 * This is just a single vertex in the virtual world,
		 * #aiFace contains just one index for such a primitive.
		 */
		PrimitiveType_POINT = 0x1,

		/** A line primitive.
		 *
		 * This is a line defined through a start and an end position.
		 * #aiFace contains exactly two indices for such a primitive.
		 */
		PrimitiveType_LINE = 0x2,

		/** A triangular primitive.
		 *
		 * A triangle consists of three indices.
		 */
		PrimitiveType_TRIANGLE = 0x4,

		/** A higher-level polygon with more than 3 edges.
		 *
		 * A triangle is a polygon, but polygon in this context means
		 * "all polygons that are not triangles". The "Triangulate"-Step
		 * is provided for your convenience, it splits all polygons in
		 * triangles (which are much easier to handle).
		 */
		PrimitiveType_POLYGON = 0x8
	};
}
