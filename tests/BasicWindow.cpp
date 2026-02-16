#include <Sprout/Sprout.h>
#include <thread>
#include <chrono>

int main(int argc, char* argv[])
{
    Sprout::WindowInfo info{};

    Sprout::Window window(info);
    std::this_thread::sleep_for(std::chrono::seconds(1));

    return 0;
}