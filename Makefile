# Makefile para gerenciamento do FrotaGo (Frontend e Backend)
# Idioma da documentação e mensagens: Português (Angola)

SHELL := /bin/bash

# Cores para saída no terminal
GREEN  := \033[1;32m
YELLOW := \033[1;33m
BLUE   := \033[1;34m
RESET  := \033[0m

# Caminhos dos subprojectos
FRONTEND_DIR := frontend
BACKEND_SLN  := backend/FrotaGo.Backend/FrotaGo.sln
BACKEND_API  := backend/FrotaGo.Backend/src/FrotaGo.Api/FrotaGo.Api.csproj

.PHONY: all help install install-frontend restore-backend build build-frontend build-backend run-frontend run-backend run

# Alvo padrão
all: help

# Ajuda / Documentação dos comandos
help:
	@printf "$(BLUE)FrotaGo - Utilitário de Execução e Compilação$(RESET)\n"
	@printf "Escolha um dos comandos abaixo para executar:\n\n"
	@printf "  $(GREEN)make install$(RESET)          - Instala dependências do Frontend e do Backend\n"
	@printf "  $(GREEN)make build$(RESET)            - Compila o Frontend e o Backend\n"
	@printf "  $(GREEN)make run$(RESET)              - Executa o Frontend e o Backend em simultâneo\n"
	@printf "  $(GREEN)make run-frontend$(RESET)     - Executa apenas o Frontend (Angular)\n"
	@printf "  $(GREEN)make run-backend$(RESET)      - Executa apenas o Backend (ASP.NET Core)\n"
	@printf "  $(GREEN)make build-frontend$(RESET)    - Compila apenas o Frontend\n"
	@printf "  $(GREEN)make build-backend$(RESET)     - Compila apenas o Backend\n"
	@printf "  $(GREEN)make install-frontend$(RESET) - Instala dependências do Frontend (npm install)\n"
	@printf "  $(GREEN)make restore-backend$(RESET)  - Restaura pacotes do Backend (dotnet restore)\n\n"

# Instalação de dependências
install: install-frontend restore-backend

install-frontend:
	@printf "$(YELLOW)Instalando dependências do Frontend (Angular)...$(RESET)\n"
	cd $(FRONTEND_DIR) && npm install

restore-backend:
	@printf "$(YELLOW)Restaurando pacotes NuGet do Backend (ASP.NET Core)...$(RESET)\n"
	dotnet restore $(BACKEND_SLN)

# Compilação
build: build-frontend build-backend

build-frontend:
	@printf "$(YELLOW)Compilando o Frontend (Angular)...$(RESET)\n"
	cd $(FRONTEND_DIR) && npm run build

build-backend:
	@printf "$(YELLOW)Compilando o Backend (ASP.NET Core)...$(RESET)\n"
	dotnet build $(BACKEND_SLN)

# Execução individual
run-frontend:
	@printf "$(GREEN)Iniciando o Frontend (Angular)...$(RESET)\n"
	cd $(FRONTEND_DIR) && npm start

run-backend:
	@printf "$(GREEN)Iniciando o Backend (ASP.NET Core)...$(RESET)\n"
	dotnet run --project $(BACKEND_API)

# Execução simultânea (concorrência)
run:
	@printf "$(GREEN)Iniciando o Backend e o Frontend em simultâneo...$(RESET)\n"
	@printf "$(YELLOW)Pressione Ctrl+C para encerrar ambos os serviços.$(RESET)\n\n"
	@trap 'kill 0' SIGINT; \
	(cd $(FRONTEND_DIR) && npm start) & \
	(dotnet run --project $(BACKEND_API)) & \
	wait
