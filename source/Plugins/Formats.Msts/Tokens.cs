//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// ReSharper disable UnusedMember.Global
namespace OpenBve.Formats.MsTs
{
	/// <summary>The tokens from the Kuju compressed binary format</summary>
    public enum KujuTokenID : uint
    {
        /*
		* utils\FFEDIT\coreids.tok
        * NOTE: This only includes object based tokens, not the extended list
        * 
		*/
        error = 0,
        comment = 1,
        point,
        vector,
        quat,
        normals,
        normal_idxs,
        points,
        uv_point,
        uv_points,
        colour,
        colours,
        packed_colour,
        image,
        images,
        texture,
        textures,
        light_material,
        light_materials,
        linear_key,
        tcb_key,
        linear_pos,
        tcb_pos,
        slerp_rot,
        tcb_rot,
        controllers,
        anim_node,
        anim_nodes,
        animation,
        animations,
        anim,
        lod_controls,
        lod_control,
        distance_levels_header,
        distance_level_header,
        dlevel_selection,
        distance_levels,
        distance_level,
        sub_objects,
        sub_object,
        sub_object_header,
        geometry_info,
        geometry_nodes,
        geometry_node,
        geometry_node_map,
        cullable_prims,
        vtx_state,
        vtx_states,
        vertex,
        vertex_uvs,
        vertices,
        vertex_set,
        vertex_sets,
        primitives,
        prim_state,
        prim_states,
        prim_state_idx,
        indexed_point_list,
        point_list,
        indexed_line_list,
        indexed_trilist,
        tex_idxs,
        tri,
        vertex_idxs,
        flags,
        matrix,
        matrices,
        hierarchy,
        volumes,
        vol_sphere,
        shape_header,
        shape,
        shader_names,
        shader_name,
        texture_filter_names,
        texture_filter_name,
        sort_vectors,
        uvop_arg_sets,
        uvop_arg_set,
        light_model_cfgs,
        light_model_cfg,
        uv_ops,
        uvop_copy,
        uv_op_share,
        uv_op_copy,
        uv_op_uniformscale,
        uv_op_user_uninformscale,
        uv_op_nonuniformscale,
        uv_op_user_nonuninformscale,
        uv_op_transform,
        uv_op_user_transform,
        uv_op_reflectxy,
        uv_op_reflectmap,
        uv_op_reflectmapfull,
        uv_op_spheremap,
        uv_op_spheremapfull,
        uv_op_specularmap,
        uv_op_embossbump,
        user_uv_args,
        io_dev,
        io_map,
        sguid,
        dlev_cfg_table,
        dlev_cfg,
        subobject_shaders,
        subobject_light_cfgs,
        shape_named_data,
        shape_named_data_header,
        shape_named_geometry,
        shape_geom_ref,
        material_palette,
        blend_config,
        blend_config_header,
        filtermode_cfgs,
        filter_mode_cfg,
        blend_mode_cfgs,
        blend_mode_cfg,
        texture_stage_progs,
        texture_stage_prog,
        blend_mode_cfg_refs,
        shader_cfgs,
        shader_cfg,
        texture_slots,
        texture_slot,
        named_filter_modes,
        named_filter_mode,
        filtermode_cfg_refs,
        filtermode_cfg_ref,
        named_shaders,
        named_shader,
        shader_cfg_refs,
        shader_cfg_ref,
    }
}
