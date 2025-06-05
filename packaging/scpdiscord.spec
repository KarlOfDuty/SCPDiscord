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

%{__install} -d %{buildroot}/usr/lib/systemd/system
%{__install} -m 644 %{repo_root}/packaging/scpdiscord.service %{buildroot}/usr/lib/systemd/system/

%{__install} -d %{buildroot}/etc/scpdiscord/
%{__install} -m 600 %{repo_root}/SCPDiscordBot/default_config.yml %{buildroot}/etc/scpdiscord/config.yml

%{__install} -d %{buildroot}/var/lib/scpdiscord
%{__install} -d %{buildroot}/var/log/scpdiscord

%pre
getent group scpdiscord > /dev/null || groupadd scpdiscord
getent passwd scpdiscord > /dev/null || useradd -r -m -d /var/lib/scpdiscord -s /sbin/nologin -g scpdiscord scpdiscord

%post
SYSTEMD_VERSION=$(systemctl --version | awk '{if($1=="systemd" && $2~"^[0-9]"){print $2}}' | head -n 1)
if (( $SYSTEMD_VERSION < 253 )); then
    echo "Systemd version is lower than 253 ($SYSTEMD_VERSION); using legacy service type 'notify' instead of 'notify-reload'"
    sed -i 's/^Type=notify-reload$/Type=notify/' "/usr/lib/systemd/system/scpdiscord.service"
fi
%systemd_post scpdiscord.service

%preun
%systemd_preun scpdiscord.service

%postun
%systemd_postun_with_restart scpdiscord.service

%files
%attr(0755,root,root) /usr/bin/scpdiscord
%attr(0644,root,root) /usr/lib/systemd/system/scpdiscord.service
%config %attr(0600, scpdiscord, scpdiscord) /etc/scpdiscord/config.yml
%dir %attr(0700, scpdiscord, scpdiscord) /var/lib/scpdiscord
%dir %attr(0755, scpdiscord, scpdiscord) /var/log/scpdiscord