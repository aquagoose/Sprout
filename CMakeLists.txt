cmake_minimum_required(VERSION 4.1)
project(Sprout)

set(CMAKE_CXX_STANDARD 17)

file(GLOB SPROUT_SRC CONFIGURE_DEPENDS
        include/Sprout/*.h
        src/*.h
        src/*.cpp
)

file(GLOB SPROUT_GRAPHICS_SRC CONFIGURE_DEPENDS
        include/Sprout/Graphics/*.h
        src/Graphics/*.h
        src/Graphics/*.cpp
)
list(APPEND SPROUT_SRC ${SPROUT_GRAPHICS_SRC})

add_library(Sprout STATIC ${SPROUT_SRC})
target_include_directories(Sprout PUBLIC include)

add_subdirectory(external/SDL)
target_link_libraries(Sprout PUBLIC SDL3::SDL3)