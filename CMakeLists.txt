cmake_minimum_required(VERSION 4.1)
project(Sprout)

set(CMAKE_CXX_STANDARD 17)

file(GLOB SPROUT_SRC CONFIGURE_DEPENDS include/*.h)

add_library(Sprout STATIC ${SPROUT_SRC})
target_include_directories(Sprout PUBLIC include/)

add_subdirectory(lib/SDL)
target_link_libraries(Sprout PUBLIC SDL3::SDL3)