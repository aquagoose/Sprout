cmake_minimum_required(VERSION 4.1)
project(Sprout)

set(CMAKE_CXX_STANDARD 17)

file(GLOB SPROUT_SRC CONFIGURE_DEPENDS include/Sprout/*.h src/*.cpp)

add_library(Sprout STATIC ${SPROUT_SRC})
target_include_directories(Sprout PUBLIC include/)

add_subdirectory(lib/SDL)
target_link_libraries(Sprout PUBLIC SDL3::SDL3)

message(STATUS "Searching for tests.")
file(GLOB TEST_SRC tests/*.cpp)
foreach (file IN LISTS TEST_SRC)
    get_filename_component(filename ${file} NAME_WE)
    set(testname Test_${filename})
    message(STATUS "${file} -> ${testname}")
    add_executable(${testname} ${file})
    target_link_libraries(${testname} PRIVATE Sprout)
endforeach ()