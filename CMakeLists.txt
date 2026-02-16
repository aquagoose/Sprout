cmake_minimum_required(VERSION 4.1)
project(Sprout)

set(CMAKE_CXX_STANDARD 17)

option(SPROUT_RENDERER_OPENGL "Enable the OpenGL renderer backend" ON)

file(GLOB SPROUT_SRC CONFIGURE_DEPENDS
        include/Sprout/*.h
        src/*.cpp
        include/Sprout/Graphics/*.h
        src/Graphics/*.cpp
)

if (SPROUT_RENDERER_OPENGL)
    file(GLOB SPROUT_OPENGL_SRC CONFIGURE_DEPENDS src/Graphics/GL/*.h src/Graphics/GL/*.cpp)
    list(APPEND SPROUT_SRC ${SPROUT_OPENGL_SRC})
    add_compile_definitions(SPROUT_RENDERER_OPENGL)
endif ()

add_library(Sprout STATIC ${SPROUT_SRC})
target_include_directories(Sprout PUBLIC include/)

add_subdirectory(lib/SDL)
target_link_libraries(Sprout PUBLIC SDL3::SDL3)

add_subdirectory(lib/glad)
target_link_libraries(Sprout PUBLIC glad)

message(STATUS "Searching for tests.")
file(GLOB TEST_SRC tests/*.cpp)
foreach (file IN LISTS TEST_SRC)
    get_filename_component(filename ${file} NAME_WE)
    set(testname Test_${filename})
    message(STATUS "${file} -> ${testname}")
    add_executable(${testname} ${file})
    target_link_libraries(${testname} PRIVATE Sprout)
endforeach ()