%global debug_package %{nil}
%global repo_root %{_topdir}/..
%global base_version %(echo "$(sed -ne '/Version/{s/.*<Version>\\(.*\\)<\\/Version>.*/\\1/p;q;}' < SCPDiscordBot/SCPDiscordBot.csproj)")

%if %{defined dev_build}
Name:       scpdiscord-dev
Summary:    A very customisable Discord bot + SCP:SL plugin combo (dev build)
Version:    %{base_version}~%(date "+%%Y%%m%%d%%H%%M%%S")git%(git rev-parse --short HEAD)
Provides:   scpdiscord
%else
Name:       scpdiscord
Summary:    A very customisable Discord bot + SCP:SL plugin combo
Version:    %{base_version}
%endif
Release:    1%{?dist}
License:    GPLv3
URL:        https://github.com/KarlOfDuty/SCPDiscord
Packager:   KarlofDuty
Source:     rpm-source.tar.gz

BuildRequires: systemd-rpm-macros
Requires: dotnet-runtime-9.0
%{?systemd_requires}

%description
SCPDiscord is a very customisable Discord bot + SCP:SL plugin combo
which lets you monitor and manage your SCP:SL servers from Discord.

%prep
%setup -T -c

%build
dotnet publish %{repo_root}/SCPDiscordBot/SCPDiscordBot.csproj -p:PublishSingleFile=true -r linux-x64 -c Release --self-contained false --output %{_builddir}/out

%install
if [[ -d %{_rpmdir}/%{_arch} ]]; then
  %{__rm} %{_rpmdir}/%{_arch}/*
fi

%{__install} -d %{buildroot}/usr/bin
# rpmbuild post-processing using the strip command breaks dotnet binaries, remove the executable bit to avoid it
%{__install} -m 644 %{_builddir}/out/scpdiscord %{buildroot}/usr/bin/scpdiscord

%{__install} -d %{buildroot}/usr/lib/systemd/system/
%{__install} -m 644 %{repo_root}/packaging/scpdiscord@.service %{buildroot}/usr/lib/systemd/system/

%{__install} -d %{buildroot}/usr/share/scpdiscord/
%{__install} -m 644 %{repo_root}/SCPDiscordBot/default_config.yml %{buildroot}/usr/share/scpdiscord/

%{__install} -D -m 644 %{repo_root}/packaging/scpdiscord.sysusers %{buildroot}%{_sysusersdir}/scpdiscord.conf
%{__install} -D -m 644 %{repo_root}/packaging/scpdiscord.tmpfiles %{buildroot}%{_tmpfilesdir}/scpdiscord.conf

%post
SYSTEMD_VERSION=$(systemctl --version | awk '{if($1=="systemd" && $2~"^[0-9]"){print $2}}' | head -n 1)
if [ -n "$SYSTEMD_VERSION" ] && [ "$SYSTEMD_VERSION" -lt 253 ]; then
    echo "Systemd version is lower than 253 ($SYSTEMD_VERSION); using legacy service type 'notify' instead of 'notify-reload'"
    sed -i 's/^Type=notify-reload$/Type=notify/' "/usr/lib/systemd/system/scpdiscord@.service"
fi
%sysusers_create_compat %{_sysusersdir}/scpdiscord.conf
%tmpfiles_create %{_tmpfilesdir}/scpdiscord.conf
%systemd_post scpdiscord@.service

%preun
%systemd_preun scpdiscord@.service

%postun
%systemd_postun scpdiscord@.service

%files
%{_sysusersdir}/scpdiscord.conf
%{_tmpfilesdir}/scpdiscord.conf

%attr(0755,root,root) /usr/bin/scpdiscord
/usr/lib/systemd/system/scpdiscord@.service
/usr/share/scpdiscord/default_config.yml