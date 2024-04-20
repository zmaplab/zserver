#!/bin/bash
set -e

# 在这里添加你需要执行的命令
# 例如，运行数据库迁移、配置检查等
fc-cache -f -v

# 执行 Dockerfile 中 CMD 指定的命令
exec "$@"