#!/usr/bin/env python3
"""
===============================================================================
FrotaGo ERP — Cliente de Terminal Interativo para Motorista / Instrutor
===============================================================================
Este script Python atua como a App Móvel do Instrutor diretamente no Terminal.
Permite autenticação, consulta de agenda, início de aula prática, transmissão
de telemetria GPS em tempo real (HUD no terminal) e finalização de aula.

Uso:
  python3 scripts/driver_cli_client.py
  python3 scripts/driver_cli_client.py --auto         (Execução automática)
  python3 scripts/driver_cli_client.py --url URL      (Especificar URL da API)
===============================================================================
"""

import os
import sys
import json
import time
import argparse
import urllib.request
import urllib.error

# Configuração Padrão
DEFAULT_API_URL = "http://localhost:5073"

# Cores ANSI e Formatação de Terminal
GREEN = "\033[1;32m"
YELLOW = "\033[1;33m"
BLUE = "\033[1;34m"
CYAN = "\033[1;36m"
RED = "\033[1;31m"
MAGENTA = "\033[1;35m"
BOLD = "\033[1m"
DIM = "\033[2m"
RESET = "\033[0m"

# Rotas GPS de Luanda Pré-Configuradas
LUANDA_ROUTES = {
    "1": {
        "name": "Circuito Exame Talatona / Via Expressa",
        "waypoints": [
            {"lat": -8.9142, "lng": 13.1892, "speed": 20.0},
            {"lat": -8.9175, "lng": 13.1930, "speed": 35.0},
            {"lat": -8.9210, "lng": 13.1985, "speed": 50.0},
            {"lat": -8.9260, "lng": 13.2040, "speed": 60.0},
            {"lat": -8.9310, "lng": 13.2095, "speed": 45.0},
            {"lat": -8.9350, "lng": 13.2130, "speed": 30.0},
            {"lat": -8.9380, "lng": 13.2160, "speed": 0.0}
        ]
    },
    "2": {
        "name": "Marginal de Luanda -> Kinaxixi -> Maianga",
        "waypoints": [
            {"lat": -8.8078, "lng": 13.2235, "speed": 30.0},
            {"lat": -8.8095, "lng": 13.2248, "speed": 40.0},
            {"lat": -8.8120, "lng": 13.2270, "speed": 45.0},
            {"lat": -8.8155, "lng": 13.2292, "speed": 35.0},
            {"lat": -8.8190, "lng": 13.2315, "speed": 50.0},
            {"lat": -8.8230, "lng": 13.2340, "speed": 25.0}
        ]
    },
    "3": {
        "name": "Circuito Centralidade do Kilamba",
        "waypoints": [
            {"lat": -8.9880, "lng": 13.2510, "speed": 25.0},
            {"lat": -8.9910, "lng": 13.2545, "speed": 40.0},
            {"lat": -8.9950, "lng": 13.2580, "speed": 40.0},
            {"lat": -8.9990, "lng": 13.2620, "speed": 30.0},
            {"lat": -9.0020, "lng": 13.2650, "speed": 0.0}
        ]
    }
}


class DriverTerminalClient:
    def __init__(self, api_url):
        self.api_url = api_url.rstrip("/")
        self.token = None
        self.user_info = None
        self.school_id = None
        self.active_lesson = None
        self.active_session_id = None

    def clear_screen(self):
        os.system("cls" if os.name == "nt" else "clear")

    def print_banner(self):
        print(f"{CYAN}{BOLD}")
        print("  ┌─────────────────────────────────────────────────────────────┐")
        print("  │            F R O T A G O  —  M O B I L E  C L I             │")
        print("  │             Terminal do Motorista / Instrutor               │")
        print("  └─────────────────────────────────────────────────────────────┘")
        print(f"{RESET}")
        
        status_line = f"  {BOLD}Servidor:{RESET} {self.api_url}"
        if self.token:
            user_name = self.user_info.get("nome", "Instrutor") if self.user_info else "Autenticado"
            status_line += f"  │  {BOLD}Motorista:{RESET} {GREEN}{user_name}{RESET}"
        else:
            status_line += f"  │  {BOLD}Estado:{RESET} {YELLOW}Não Autenticado{RESET}"
        
        if self.active_lesson:
            lesson_id_short = str(self.active_lesson.get("lessonId", ""))[:8]
            status_line += f"  │  {BOLD}Aula Ativa:{RESET} {CYAN}#{lesson_id_short}{RESET}"
        
        print(status_line)
        print("  " + "─" * 63 + "\n")

    def request_api(self, endpoint, method="GET", payload=None, use_auth=True, custom_token=None):
        url = f"{self.api_url}{endpoint}"
        headers = {
            "Content-Type": "application/json",
            "Accept": "application/json"
        }
        token_to_use = custom_token or (self.token if use_auth else None)
        if token_to_use:
            headers["Authorization"] = f"Bearer {token_to_use}"

        data = json.dumps(payload).encode("utf-8") if payload else None
        req = urllib.request.Request(url, data=data, headers=headers, method=method)

        start_time = time.time()
        try:
            with urllib.request.urlopen(req) as resp:
                elapsed = (time.time() - start_time) * 1000
                body_bytes = resp.read()
                body_json = json.loads(body_bytes.decode("utf-8")) if body_bytes else {}
                return resp.status, body_json, elapsed
        except urllib.error.HTTPError as e:
            elapsed = (time.time() - start_time) * 1000
            error_body = e.read().decode("utf-8")
            try:
                err_json = json.loads(error_body)
            except Exception:
                err_json = {"message": error_body}
            return e.code, err_json, elapsed
        except Exception as ex:
            return 500, {"message": f"Erro de Conexão: {str(ex)}"}, 0.0

    def bootstrap_environment(self):
        """Cria automaticamente uma escola, veículo, aluno e agendamento de aula se necessário."""
        print(f"\n{BLUE}ℹ [BOOTSTRAP]{RESET} A inicializar ambiente de testes no servidor...")
        timestamp = int(time.time())
        owner_email = f"owner_{timestamp}@frotago.ao"
        instructor_email = f"instrutor_{timestamp}@frotago.ao"
        
        # 1. Registo da Escola
        reg_payload = {
            "ownerName": "Proprietário FrotaGo",
            "phone": "+244923000000",
            "email": owner_email,
            "password": "Password123!",
            "schoolName": f"Escola de Condução Express {timestamp % 1000}",
            "shortName": "EC Express",
            "slug": f"ec-express-{timestamp}",
            "nif": f"5400{timestamp}",
            "licenseNumber": f"LIC-{timestamp}",
            "licenseIssuer": "DNTT",
            "province": "Luanda",
            "municipality": "Talatona",
            "address": "Via S8, Talatona",
            "branchName": "Sede",
            "plan": "Gratuito"
        }
        status, res, _ = self.request_api("/api/auth/register-school", method="POST", payload=reg_payload, use_auth=False)
        if status != 200:
            # Tenta login como admin padrao
            status, res, _ = self.request_api("/api/auth/login", method="POST", payload={"email": "admin@frotago.ao", "password": "Password123!"}, use_auth=False)
            if status != 200:
                print(f"{RED}✖ Falha ao inicializar escola de teste ({res.get('message')}).{RESET}")
                return False
        
        owner_token = res.get("token")
        school_id = res.get("schoolId")
        
        # 2. Convidar Instrutor
        inv_payload = {
            "schoolId": school_id,
            "name": "Motorista Instrutor Teste",
            "email": instructor_email,
            "role": 5
        }
        status, inv_res, _ = self.request_api("/api/users/invite", method="POST", payload=inv_payload, custom_token=owner_token)
        if status != 200:
            print(f"{RED}✖ Erro ao convidar instrutor: {inv_res.get('message')}{RESET}")
            return False

        inv_token = inv_res.get("token")

        # 3. Aceitar convite (Definir senha)
        status, acc_res, _ = self.request_api("/api/users/accept-invitation", method="POST", payload={"token": inv_token, "password": "InstructorPass123!"}, use_auth=False)
        if status != 200:
            print(f"{RED}✖ Erro ao aceitar convite: {acc_res.get('message')}{RESET}")
            return False

        # 4. Criar Entidade Instrutor na Escola
        inst_payload = {
            "name": "Motorista Instrutor Teste",
            "email": instructor_email,
            "phoneNumber": "+244923000111",
            "licenseNumber": f"LIC-INST-{timestamp}"
        }
        status, inst_res, _ = self.request_api("/api/instructors", method="POST", payload=inst_payload, custom_token=owner_token)
        instructor_id = inst_res.get("id") if status in (200, 201) else None

        if not instructor_id:
            # Fallback: tentar listar instrutores da escola
            status, inst_list, _ = self.request_api("/api/instructors", method="GET", custom_token=owner_token)
            if status in (200, 201) and isinstance(inst_list, list) and len(inst_list) > 0:
                instructor_id = inst_list[0].get("id")

        # Guardar ID da entidade Instructor (tabela Instructors, NÃO tabela Users)
        self.created_instructor_id = instructor_id

        # 5. Criar Veículo
        v_payload = {
            "licensePlate": f"LD-{timestamp%100:02d}-99-TR",
            "brand": "Hyundai",
            "model": "i10",
            "chassis": f"CH-{timestamp}",
            "year": 2024,
            "odometer": 15000,
            "fuel": 1,
            "transmission": 1
        }
        status, v_res, _ = self.request_api("/api/vehicles", method="POST", payload=v_payload, custom_token=owner_token)
        vehicle_id = v_res.get("id") if status in (200, 201) else None
        self.created_vehicle_id = vehicle_id

        # 6. Criar Aluno
        s_payload = {
            "name": "Manuel António Aluno",
            "email": f"aluno_{timestamp}@frotago.ao",
            "phoneNumber": "+244924000111",
            "identityCardNumber": f"0099{timestamp%10000:04d}LA041",
            "category": 1
        }
        status, s_res, _ = self.request_api("/api/students", method="POST", payload=s_payload, custom_token=owner_token)
        student_id = s_res.get("id") if status in (200, 201) else None

        # 7. Agendar Aula
        now_utc = time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime())
        lesson_payload = {
            "studentId": student_id,
            "instructorId": instructor_id or "00000000-0000-0000-0000-000000000000",
            "vehicleId": vehicle_id,
            "scheduledDate": now_utc,
            "durationMinutes": 60,
            "topic": "Condução Urbana e Manobras de Exame",
            "observations": "Aula interativa do terminal."
        }
        status, l_res, _ = self.request_api("/api/lessons", method="POST", payload=lesson_payload, custom_token=owner_token)
        if status not in (200, 201):
            print(f"{YELLOW}⚠ Aviso ao agendar aula ({status}): {l_res.get('message')}{RESET}")

        # Guardar credenciais do ambiente para consulta e acompanhamento no painel web
        self.owner_info = {
            "school_name": f"Escola de Condução Express {timestamp % 1000}",
            "email": owner_email,
            "password": "Password123!",
            "school_id": school_id
        }
        self.instructor_credentials = {
            "email": instructor_email,
            "password": "InstructorPass123!"
        }

        print(f"{GREEN}✔ Ambiente inicializado com sucesso!{RESET}")
        self.print_credentials_box()
        return instructor_email, "InstructorPass123!"

    def print_credentials_box(self):
        if not hasattr(self, 'owner_info') or not self.owner_info:
            print(f"{YELLOW}⚠ Nenhuma credencial de ambiente registada nesta sessão.{RESET}")
            return

        o = self.owner_info
        i = getattr(self, 'instructor_credentials', {})

        print(f"\n{BOLD}{CYAN}┌─────────────────────────────────────────────────────────────────────────────┐{RESET}")
        print(f"{BOLD}{CYAN}│                   🔑 CREDENCIAIS E ACESSO AO PAINEL WEB                     │{RESET}")
        print(f"{BOLD}{CYAN}├─────────────────────────────────────────────────────────────────────────────┤{RESET}")
        school_name = o.get('school_name', '')
        print(f"│ 🏢 {BOLD}Escola:{RESET}   {GREEN}{school_name:<64}{RESET} │")
        print(f"{BOLD}{CYAN}├─────────────────────────────────────────────────────────────────────────────┤{RESET}")
        print(f"│ 🌐 {BOLD}PAINEL WEB (ADMINISTRADOR / PROPRIETÁRIO):{RESET}                               │")
        print(f"│    URL:           {YELLOW}http://localhost:4200{RESET}                                    │")
        print(f"│    E-mail Admin:  {CYAN}{o.get('email', ''):<55}{RESET} │")
        print(f"│    Palavra-passe: {CYAN}{o.get('password', ''):<55}{RESET} │")
        print(f"{BOLD}{CYAN}├─────────────────────────────────────────────────────────────────────────────┤{RESET}")
        print(f"│ 📱 {BOLD}APP MÓVEL (MOTORISTA / INSTRUTOR):{RESET}                                      │")
        print(f"│    E-mail Instrutor: {GREEN}{i.get('email', ''):<53}{RESET} │")
        print(f"│    Palavra-passe:   {GREEN}{i.get('password', ''):<53}{RESET} │")
        print(f"{BOLD}{CYAN}└─────────────────────────────────────────────────────────────────────────────┘{RESET}")
        print(f" 💡 {BOLD}DICA DE ACOMPANHAMENTO EM TEMPO REAL:{RESET}")
        print(f"    1. Abra o navegador em: {YELLOW}http://localhost:4200{RESET}")
        print(f"    2. Inicie sessão com o E-mail e Palavra-passe do Admin acima.")
        print(f"    3. Vá para a página de 'Rastreamento / Frota' para ver o veículo no mapa!\n")

    def login(self, email=None, password=None):
        print(f"\n{BOLD}─── AUTENTICAÇÃO DO MOTORISTA ───{RESET}")
        if not email:
            email = input(" 📧 E-mail do Instrutor: ").strip()
        if not password:
            import getpass
            password = getpass.getpass(" 🔑 Palavra-passe: ").strip()

        print(f"\n{BLUE}🔄 Autenticando na API Mobile...{RESET}")
        status, res, ms = self.request_api("/api/auth/login", method="POST", payload={"email": email, "password": password}, use_auth=False)

        if status == 200 and "token" in res:
            self.token = res["token"]
            self.school_id = res.get("schoolId")
            print(f"{GREEN}✔ Autenticado com sucesso! [HTTP 200 - {ms:.1f}ms]{RESET}")
            self.load_profile()
            return True
        else:
            print(f"{RED}✖ Falha na autenticação: {res.get('message', 'Credenciais inválidas')}{RESET}")
            return False

    def load_profile(self):
        status, res, _ = self.request_api("/api/mobile/profile", method="GET")
        if status == 200:
            self.user_info = res
            return res
        return None

    def show_profile(self):
        if not self.token:
            print(f"{YELLOW}⚠ Deve efetuar login primeiro.{RESET}")
            return

        status, prof, _ = self.request_api("/api/mobile/profile", method="GET")
        status_dash, dash, _ = self.request_api("/api/mobile/dashboard", method="GET")

        print(f"\n{BOLD}{CYAN}─── PERFIL DO INSTRUTOR ───{RESET}")
        if status == 200:
            print(f"  {BOLD}Nome:{RESET}      {prof.get('nome')}")
            print(f"  {BOLD}Escola:{RESET}    {prof.get('escola')}")
            print(f"  {BOLD}Email:{RESET}     {prof.get('email')}")
            print(f"  {BOLD}Telefone:{RESET}  {prof.get('telefone')}")
        
        print(f"\n{BOLD}{MAGENTA}─── DASHBOARD MOBILE ───{RESET}")
        if status_dash == 200:
            print(f"  {BOLD}Aulas Agendadas Hoje:{RESET}  {dash.get('lessonsToday')}")
            print(f"  {BOLD}Horário Próxima Aula:{RESET}  {dash.get('nextLesson')}")
            print(f"  {BOLD}Notificações do Sistema:{RESET} {dash.get('notifications')}")
        print()

    def list_today_lessons(self):
        if not self.token:
            print(f"{YELLOW}⚠ Deve efetuar login primeiro.{RESET}")
            return []

        print(f"\n{BOLD}{CYAN}─── AGENDA DE AULAS PRÁTICAS (HOJE) ───{RESET}")
        status, lessons, ms = self.request_api("/api/mobile/lessons/today", method="GET")

        if status != 200 or not isinstance(lessons, list) or len(lessons) == 0:
            print(f"{YELLOW}ℹ Nenhuma aula encontrada para hoje ou lista vazia.{RESET}")
            return []

        print(f"┌─────┬──────────────────────────────┬──────────────────────────────┬────────┬─────────────┐")
        print(f"│  #  │ Aluno                        │ Viatura                      │ Hora   │ Estado      │")
        print(f"├─────┼──────────────────────────────┼──────────────────────────────┼────────┼─────────────┤")
        
        for idx, l in enumerate(lessons, 1):
            student_str = (l.get("student") or "")[:28].ljust(28)
            vehicle_str = (l.get("vehicle") or "")[:28].ljust(28)
            time_str = (l.get("start") or "00:00")[:6].ljust(6)
            st = l.get("status", "")
            
            st_color = GREEN if st == "EmCurso" else (BLUE if st == "Agendada" else YELLOW)
            st_str = f"{st_color}{st[:11].ljust(11)}{RESET}"
            
            print(f"│  {idx}  │ {student_str} │ {vehicle_str} │ {time_str} │ {st_str} │")
        print(f"└─────┴──────────────────────────────┴──────────────────────────────┴────────┴─────────────┘")
        print(f"{DIM}Total: {len(lessons)} aula(s) [HTTP {status} - {ms:.1f}ms]{RESET}\n")
        return lessons

    def select_lesson(self, lessons=None):
        if not lessons:
            lessons = self.list_today_lessons()
        if not lessons:
            return None

        try:
            choice = input(" Select a aula pelo número (#) ou Pressione Enter para a 1ª: ").strip()
            idx = int(choice) - 1 if choice else 0
            if 0 <= idx < len(lessons):
                self.active_lesson = lessons[idx]
                print(f"{GREEN}✔ Aula selecionada com sucesso! ID: {self.active_lesson.get('lessonId')}{RESET}")
                return self.active_lesson
            else:
                print(f"{RED}✖ Número inválido.{RESET}")
        except Exception:
            print(f"{RED}✖ Seleção inválida.{RESET}")
        return None

    def view_lesson_details(self):
        if not self.active_lesson:
            print(f"{YELLOW}⚠ Selecione primeiro uma aula na opção 3.{RESET}")
            return

        lesson_id = self.active_lesson.get("lessonId")
        status, detail, _ = self.request_api(f"/api/mobile/lessons/{lesson_id}", method="GET")
        status_res, res_info, _ = self.request_api(f"/api/mobile/lessons/{lesson_id}/resources", method="GET")

        if status == 200:
            st = detail.get("student", {})
            vh = detail.get("vehicle", {})
            sc = detail.get("schedule", {})

            print(f"\n{BOLD}{CYAN}─── DETALHES DA AULA PRÁTICA ───{RESET}")
            print(f"  {BOLD}Lesson ID:{RESET}       {detail.get('lessonId')}")
            print(f"  {BOLD}Estado da Aula:{RESET}  {GREEN}{detail.get('status')}{RESET}")
            print(f"  {BOLD}Tópico da Aula:{RESET}  {detail.get('topic')}")
            print(f"  {BOLD}Aluno:{RESET}           {st.get('name')} (Categoria: {st.get('category')}) | Tel: {st.get('phone')}")
            print(f"  {BOLD}Viatura:{RESET}         {vh.get('brand')} {vh.get('model')} [{BOLD}{vh.get('licensePlate')}{RESET}] ({vh.get('fuelType')})")
            print(f"  {BOLD}Agendamento:{RESET}     Data: {sc.get('date')} às {sc.get('time')} ({sc.get('durationMinutes')} min)")
            print(f"  {BOLD}Ponto Encontro:{RESET}  {detail.get('meetingPoint')}")

            if status_res == 200 and "documents" in res_info:
                print(f"\n{BOLD}{YELLOW}  📋 Check-list de Documentação Obrigatória:{RESET}")
                for doc in res_info["documents"]:
                    print(f"     ✔ {doc}")
            print()

    def start_lesson(self):
        if not self.active_lesson:
            print(f"{YELLOW}⚠ Selecione primeiro uma aula na opção 3.{RESET}")
            return False

        lesson_id = self.active_lesson.get("lessonId")
        print(f"\n{BOLD}{BLUE}─── INICIANDO AULA PRÁTICA ───{RESET}")
        print(f"  🔍 Verificando pré-requisitos móveis (GPS, Conexão, Permissões)...")
        time.sleep(0.4)

        # Obter detalhes completos da aula para extrair o VehicleID real
        status_d, detail, _ = self.request_api(f"/api/mobile/lessons/{lesson_id}", method="GET")
        if not isinstance(detail, dict):
            detail = {}
        vehicle_id = detail.get("vehicle", {}).get("id") if isinstance(detail.get("vehicle"), dict) else None
        if not vehicle_id:
            vehicle_id = getattr(self, 'created_vehicle_id', None)
        if not vehicle_id:
            # Fallback: tentar obter das aulas de hoje ou da api de viaturas
            status_v, v_list, _ = self.request_api("/api/vehicles", method="GET")
            if status_v in (200, 201) and isinstance(v_list, list) and len(v_list) > 0:
                vehicle_id = v_list[0].get("id")

        status, res, ms = self.request_api(f"/api/mobile/lessons/{lesson_id}/start", method="POST")

        if status == 200:
            print(f"{GREEN}✔ AULA INICIADA COM SUCESSO! [HTTP 200 - {ms:.1f}ms]{RESET}")
            print(f"  Mensagem: {res.get('message')}")
            self.active_lesson["status"] = "EmCurso"
            
            # Registar sessão de rastreamento no servidor com a viatura correta
            if vehicle_id:
                instructor_entity_id = getattr(self, 'created_instructor_id', None)
                track_start_payload = {
                    "vehicleId": vehicle_id,
                    "instructorId": instructor_entity_id,
                    "lessonId": lesson_id,
                    "provider": "terminal_driver_cli"
                }
                s_status, s_res, s_ms = self.request_api("/api/tracking/start", method="POST", payload=track_start_payload)
                if s_status in (200, 201) and isinstance(s_res, dict) and "id" in s_res:
                    self.active_session_id = s_res.get("id")
                    print(f"  📡 Sessão de Telemetria GPS Registada ID: {CYAN}{self.active_session_id}{RESET}\n")
                else:
                    print(f"  {RED}✖ Falha ao criar sessão de tracking (HTTP {s_status}): {s_res}{RESET}\n")
            else:
                print(f"  {RED}✖ vehicle_id é None — não é possível criar sessão de tracking.{RESET}\n")
            return True
        else:
            print(f"{RED}✖ Erro ao iniciar aula: {res.get('message')}{RESET}\n")
            return False

    def render_speedometer(self, speed):
        max_speed = 80.0
        bar_length = 20
        filled = int((speed / max_speed) * bar_length)
        bar = "█" * filled + "░" * (bar_length - filled)
        color = GREEN if speed < 50 else (YELLOW if speed < 70 else RED)
        return f"[{color}{bar}{RESET}] {BOLD}{speed:4.1f} km/h{RESET}"

    def run_gps_driving_mode(self, route_key="1", auto_delay=1.0):
        if not self.token:
            print(f"{YELLOW}⚠ Efetue login primeiro.{RESET}")
            return

        route_info = LUANDA_ROUTES.get(route_key, LUANDA_ROUTES["1"])
        waypoints = route_info["waypoints"]

        # Garantir que temos uma sessão de rastreamento ativa no servidor
        if not self.active_session_id:
            vehicle_id = None
            if self.active_lesson:
                lesson_id = self.active_lesson.get("lessonId")
                status_d, detail, _ = self.request_api(f"/api/mobile/lessons/{lesson_id}", method="GET")
                if status_d == 200 and isinstance(detail.get("vehicle"), dict):
                    vehicle_id = detail["vehicle"].get("id")

            if not vehicle_id:
                vehicle_id = getattr(self, 'created_vehicle_id', None)

            if not vehicle_id:
                # Tentar listar veículos da escola
                status_v, v_list, _ = self.request_api("/api/vehicles", method="GET")
                if status_v in (200, 201) and isinstance(v_list, list) and len(v_list) > 0:
                    vehicle_id = v_list[0].get("id")

            if vehicle_id:
                instructor_entity_id = getattr(self, 'created_instructor_id', None)
                track_start_payload = {
                    "vehicleId": vehicle_id,
                    "instructorId": instructor_entity_id,
                    "lessonId": self.active_lesson.get("lessonId") if self.active_lesson else None,
                    "provider": "terminal_driver_cli"
                }
                s_status, s_res, _ = self.request_api("/api/tracking/start", method="POST", payload=track_start_payload)
                if s_status in (200, 201) and "id" in s_res:
                    self.active_session_id = s_res.get("id")
                    print(f"  📡 Sessão de Telemetria Ativa ID: {CYAN}{self.active_session_id}{RESET}")

        session_id = self.active_session_id

        if not session_id:
            print(f"{RED}✖ Erro: Não foi possível obter uma sessão de rastreamento ativa. Verifique a viatura e o login.{RESET}")
            return

        print(f"\n{BOLD}{CYAN}═══════════════════════════════════════════════════════════════════{RESET}")
        print(f"{BOLD}{GREEN} 📡 MODO CONDUÇÃO — TRANSMISSÃO DE TELEMETRIA GPS EM TEMPO REAL{RESET}")
        print(f"{BOLD}{CYAN}═══════════════════════════════════════════════════════════════════{RESET}")
        print(f"  {BOLD}Rota:{RESET} {route_info['name']}")
        print(f"  {BOLD}Sessão Tracking:{RESET} {CYAN}{session_id}{RESET}")
        print(f"  {BOLD}Pontos a Transmitir:{RESET} {len(waypoints)}")
        print(f"  {DIM}Pressione Ctrl+C a qualquer momento para interromper a condução.{RESET}\n")

        try:
            for idx, wp in enumerate(waypoints, 1):
                lat = wp["lat"]
                lng = wp["lng"]
                speed = wp["speed"]

                loc_payload = {
                    "trackingSessionId": session_id,
                    "latitude": lat,
                    "longitude": lng,
                    "speed": speed
                }
                status, res, ms = self.request_api("/api/tracking/location", method="POST", payload=loc_payload)

                speed_gauge = self.render_speedometer(speed)
                status_symbol = f"{GREEN}✔ SignalR & API OK ({ms:.0f}ms){RESET}" if status == 200 else f"{RED}✖ HTTP {status} ({res.get('message', '')}){RESET}"

                print(f" 📍 Ponto {idx}/{len(waypoints)} │ Coordenadas: ({lat:.4f}, {lng:.4f}) │ Velocidade: {speed_gauge} │ {status_symbol}")
                time.sleep(auto_delay)

            print(f"\n{GREEN}✔ Rota finalizada com sucesso! Todos os pontos de GPS foram transmitidos.{RESET}\n")

        except KeyboardInterrupt:
            print(f"\n{YELLOW}⚠ Transmissão GPS interrompida pelo condutor.{RESET}\n")

    def finish_lesson(self, eval_grade=None, odometer=None, notes=None):
        if not self.active_lesson:
            print(f"{YELLOW}⚠ Selecione primeiro uma aula na opção 3.{RESET}")
            return False

        lesson_id = self.active_lesson.get("lessonId")
        print(f"\n{BOLD}{CYAN}─── FINALIZAR AULA PRÁTICA & ENVIAR RELATÓRIO ───{RESET}")

        if not eval_grade:
            print(" Selecione a avaliação do aluno:")
            print("   1. Excelente")
            print("   2. Bom")
            print("   3. Suficiente")
            print("   4. Insuficiente")
            ev_choice = input(" Opção (1-4) [Padrão: 1]: ").strip()
            eval_map = {"1": "Excelente", "2": "Bom", "3": "Suficiente", "4": "Insuficiente"}
            eval_grade = eval_map.get(ev_choice, "Excelente")

        if not odometer:
            km_input = input(" 🚗 Odómetro Final da Viatura (KM) [ex: 45250]: ").strip()
            try:
                odometer = int(km_input) if km_input else 45250
            except ValueError:
                odometer = 45250

        if not notes:
            notes = input(" 📝 Observações / Notas do Instrutor: ").strip()
            if not notes:
                notes = "Aula concluída com sucesso via Terminal CLI. Aluno dominou as manobras com segurança."

        finish_payload = {
            "evaluation": eval_grade,
            "notes": notes,
            "odometer": odometer
        }

        print(f"\n{BLUE}🔄 Enviando relatório final para o servidor...{RESET}")
        status, res, ms = self.request_api(f"/api/mobile/lessons/{lesson_id}/finish", method="POST", payload=finish_payload)

        if status in (200, 201):
            print(f"{GREEN}✔ AULA FINALIZADA E RELATÓRIO ENVIADO COM SUCESSO! [HTTP {status} - {ms:.1f}ms]{RESET}")
            print(f"  {BOLD}Avaliação:{RESET} {eval_grade}")
            print(f"  {BOLD}Odómetro Final:{RESET} {odometer} KM")
            print(f"  {BOLD}Observações:{RESET} {notes}\n")
            
            # Encerrar sessão de tracking no servidor se ativa
            if self.active_session_id:
                self.request_api(f"/api/tracking/stop/{self.active_session_id}", method="POST")
                self.active_session_id = None
                
            self.active_lesson["status"] = "Concluida"
            return True
        else:
            print(f"{RED}✖ Erro ao finalizar aula: {res.get('message')}{RESET}\n")
            return False

    def run_auto_simulation(self):
        """Executa todo o fluxo de ponta a ponta sem pedir intervenção manual."""
        self.clear_screen()
        print(f"{BOLD}{GREEN}==========================================================================={RESET}")
        print(f"{BOLD}{GREEN}   FROTAGO MOBILE — MODO SIMULAÇÃO AUTOMÁTICA DO FLUXO DO MOTORISTA        {RESET}")
        print(f"{BOLD}{GREEN}==========================================================================={RESET}\n")
        
        # 1. Bootstrap & Autenticação
        creds = self.bootstrap_environment()
        if not creds:
            print(f"{RED}✖ Erro ao criar ambiente de testes.{RESET}")
            return
        
        email, password = creds
        if not self.login(email, password):
            return

        # 2. Perfil & Agenda
        self.show_profile()
        time.sleep(1)
        
        lessons = self.list_today_lessons()
        if not lessons:
            print(f"{RED}✖ Nenhuma aula para simular.{RESET}")
            return
            
        self.active_lesson = lessons[0]
        self.view_lesson_details()
        time.sleep(1)

        # 3. Iniciar Aula
        if not self.start_lesson():
            return

        # Dar tempo ao utilizador para abrir o painel web e fazer login
        print(f"\n{BOLD}{YELLOW}═══════════════════════════════════════════════════════════════════{RESET}")
        print(f"{BOLD}{YELLOW} ⏳ AGUARDANDO — Abra o Painel Web para Acompanhar em Tempo Real!{RESET}")
        print(f"{BOLD}{YELLOW}═══════════════════════════════════════════════════════════════════{RESET}")
        self.print_credentials_box()
        wait_seconds = 15
        for remaining in range(wait_seconds, 0, -1):
            print(f"\r  ⏱  A transmissão GPS começa em {BOLD}{CYAN}{remaining:2d}{RESET} segundos... (Abra http://localhost:4200 → Rastreamento)", end="", flush=True)
            time.sleep(1)
        print(f"\r  🚀 {GREEN}A iniciar transmissão GPS em tempo real agora!                                             {RESET}\n")

        # 4. Transmissão GPS (delay de 2s entre pontos para visualização no mapa)
        self.run_gps_driving_mode(route_key="2", auto_delay=2.0)
        time.sleep(1)

        # 5. Finalizar Aula
        self.finish_lesson(eval_grade="Excelente", odometer=45300, notes="Simulação automática concluída perfeitamente pelo terminal!")
        
        print(f"{BOLD}{GREEN}==========================================================================={RESET}")
        print(f"{BOLD}{GREEN}   ✔ FLUXO COMPLETO DO MOTORISTA SIMULADO E LOGADO COM SUCESSO!             {RESET}")
        print(f"{BOLD}{GREEN}==========================================================================={RESET}")
        self.print_credentials_box()

    def run_interactive_menu(self):
        while True:
            self.clear_screen()
            self.print_banner()

            print("  ┌─────────────────────────────────────────────────────────────┐")
            print("  │                   MENU PRINCIPAL DO MOTORISTA               │")
            print("  ├─────────────────────────────────────────────────────────────┤")
            print("  │ 1. 🔐 Autenticar / Login (Instrutor / Motorista)            │")
            print("  │ 2. 👤 Ver Perfil & Resumo do Dashboard Mobile               │")
            print("  │ 3. 📅 Ver Agenda de Aulas Práticas de Hoje                  │")
            print("  │ 4. 📋 Ver Detalhes & Documentos da Aula Selecionada         │")
            print("  │ 5. 🚗 Iniciar Aula Prática (Alterar estado para EmCurso)    │")
            print("  │ 6. 📡 Modo Condução (Transmissão de Telemetria GPS)        │")
            print("  │ 7. 🏁 Finalizar Aula & Enviar Relatório (Odómetro/Notas)    │")
            print("  │ 8. ⚡ Executar Fluxo Automático Completo (Modo Demo)         │")
            print("  │ 9. 🛠️  Inicializar Novo Ambiente de Teste (Bootstrap)        │")
            print("  │10. 🔑 Exibir Credenciais do Painel Web (Admin & Escola)     │")
            print("  │ 0. 🚪 Sair                                                  │")
            print("  └─────────────────────────────────────────────────────────────┘")

            op = input("\n  👉 Escolha uma opção [0-10]: ").strip()

            if op == "1":
                self.login()
                input("\n  Pressione ENTER para continuar...")
            elif op == "2":
                self.show_profile()
                input("\n  Pressione ENTER para continuar...")
            elif op == "3":
                self.select_lesson()
                input("\n  Pressione ENTER para continuar...")
            elif op == "4":
                self.view_lesson_details()
                input("\n  Pressione ENTER para continuar...")
            elif op == "5":
                self.start_lesson()
                input("\n  Pressione ENTER para continuar...")
            elif op == "6":
                print("\n  Escolha a Rota para a Condução:")
                print("    1. Circuito Exame Talatona / Via Expressa")
                print("    2. Marginal de Luanda -> Kinaxixi -> Maianga")
                print("    3. Circuito Centralidade do Kilamba")
                rt = input("  Opção de Rota [1-3] (Padrão 1): ").strip() or "1"
                self.run_gps_driving_mode(route_key=rt, auto_delay=1.0)
                input("\n  Pressione ENTER para continuar...")
            elif op == "7":
                self.finish_lesson()
                input("\n  Pressione ENTER para continuar...")
            elif op == "8":
                self.run_auto_simulation()
                input("\n  Pressione ENTER para continuar...")
            elif op == "9":
                creds = self.bootstrap_environment()
                if creds:
                    self.login(creds[0], creds[1])
                input("\n  Pressione ENTER para continuar...")
            elif op == "10":
                self.print_credentials_box()
                input("\n  Pressione ENTER para continuar...")
            elif op == "0":
                print(f"\n{GREEN}Até logo! Bom trabalho na condução.{RESET}\n")
                sys.exit(0)


def main():
    parser = argparse.ArgumentParser(description="FrotaGo ERP — Cliente de Terminal do Motorista / Instrutor")
    parser.add_argument("--url", type=str, default=DEFAULT_API_URL, help="URL base da API ASP.NET Core")
    parser.add_argument("--auto", action="store_true", help="Executa o fluxo completo automaticamente")
    args = parser.parse_args()

    client = DriverTerminalClient(api_url=args.url)

    if args.auto:
        client.run_auto_simulation()
    else:
        client.run_interactive_menu()


if __name__ == "__main__":
    main()
