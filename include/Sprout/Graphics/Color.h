#pragma once

#include "Sprout/Math/Types.h"

namespace Sprout
{
    struct Color
    {
        f32 R;
        f32 G;
        f32 B;
        f32 A;

        Color()
        {
            R = 0;
            G = 0;
            B = 0;
            A = 0;
        }

        Color(const f32 r, const f32 g, const f32 b, const f32 a = 1.0f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        Color(const u8 r, const u8 g, const u8 b, const u8 a = U8_MAX)
        {
            constexpr f32 multiplier = 1.0 / static_cast<f32>(U8_MAX);
            R = static_cast<f32>(r) * multiplier;
            G = static_cast<f32>(g) * multiplier;
            B = static_cast<f32>(b) * multiplier;
            A = static_cast<f32>(a) * multiplier;
        }
    };
}