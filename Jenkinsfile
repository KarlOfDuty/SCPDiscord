pipeline
{
  agent any

  parameters
  {
    choice(name: 'BUILD_TYPE', choices: ['dev', 'pre-release', 'release'], description: 'Choose build type (AUR release must be performed manually)')
    string(name: 'RELEASE_VERSION', defaultValue: '', description: 'Enter the git tag name to create for a (pre-)release')
  }
  stages
  {
    stage('Initialize Environment')
    {
      steps
      {
        script
        {
          env.DOTNET_CLI_HOME = "/tmp/.dotnet-bot"
          env.DEBEMAIL="xkaess22@gmail.com"
          env.DEBFULLNAME="Karl Essinger"
          env.AUR_GIT_PACKAGE="scpdiscord-git"
          env.DEV_BUILD = params.BUILD_TYPE == 'dev' ? "true" : "false"
          env.PACKAGE_NAME = params.BUILD_TYPE == 'dev' ? "scpdiscord-dev" : "scpdiscord"
          env.RPMBUILD_ARGS = params.BUILD_TYPE == 'dev' ? "--define 'dev_build true'" : ""

          common = load("${env.WORKSPACE}/ci-utilities/scripts/common.groovy")
          common.prepare_gpg_key()
        }
      }
    }
    stage('Release Pre-Checks')
    {
      when
      {
        expression { params.BUILD_TYPE != 'dev'; }
      }
      steps
      {
        script
        {
          common.verify_release_does_not_exist("KarlOfDuty/SCPDiscord", params.RELEASE_VERSION)
        }
      }
    }
    stage('Get Dependencies')
    {
      steps
      {
        script
        {
          if (env.BRANCH_NAME == 'beta')
          {
            sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta experimental validate +quit'
          }
          else
          {
            sh 'steamcmd +force_install_dir \$HOME/scpsl +login anonymous +app_update 996560 -beta public validate +quit'
          }
          sh 'ln -s "\$HOME/scpsl/SCPSL_Data/Managed" ".scpsl_libs"'

          dir(path: 'SCPDiscordBot')
          {
            sh 'dotnet restore'
          }
        }
      }
    }
    stage('Update AUR Version')
    {
      when
      {
        expression
        {
          def remoteBranch = sh(
            script: "curl -s 'https://aur.archlinux.org/cgit/aur.git/plain/.git_branch?h=${env.AUR_GIT_PACKAGE}'",
            returnStdout: true
          ).trim()
          return remoteBranch == env.BRANCH_NAME && params.BUILD_TYPE == 'dev'
        }
      }
      steps
      {
        script
        {
          common.update_aur_git_package(env.AUR_GIT_PACKAGE, "packaging/${env.AUR_GIT_PACKAGE}.pkgbuild", "packaging/scpdiscord.install")
        }
      }
    }
    stage('Build / Package')
    {
      parallel
      {
        stage('Basic Linux')
        {
          steps
          {
            dir(path: 'SCPDiscordBot')
            {
              sh 'dotnet publish -r linux-x64 -c Release -p:PublishTrimmed=true --self-contained true --no-restore --output linux-x64/'
              sh 'mv linux-x64/scpdiscord linux-x64/scpdiscord-sc'
              sh 'dotnet publish -r linux-x64 -c Release --self-contained false --no-restore --output linux-x64/'
            }
            archiveArtifacts(artifacts: 'SCPDiscordBot/linux-x64/scpdiscord', caseSensitive: true)
            archiveArtifacts(artifacts: 'SCPDiscordBot/linux-x64/scpdiscord-sc', caseSensitive: true)
            script
            {
              env.BASIC_LINUX_PATH = 'SCPDiscordBot/linux-x64/scpdiscord'
              env.BASIC_LINUX_SC_PATH = 'SCPDiscordBot/linux-x64/scpdiscord-sc'
            }
          }
        }
        stage('Basic Windows')
        {
          steps
          {
            dir(path: 'SCPDiscordBot')
            {
              sh 'dotnet publish -r win-x64 -c Release -p:PublishTrimmed=true --self-contained true --no-restore --output windows-x64/'
              sh 'mv windows-x64/scpdiscord.exe windows-x64/scpdiscord-sc.exe'
              sh 'dotnet publish -r win-x64 -c Release --self-contained false --no-restore --output windows-x64/'
            }
            archiveArtifacts(artifacts: 'SCPDiscordBot/windows-x64/scpdiscord.exe', caseSensitive: true)
            archiveArtifacts(artifacts: 'SCPDiscordBot/windows-x64/scpdiscord-sc.exe', caseSensitive: true)
            script
            {
              env.BASIC_WINDOWS_PATH = 'SCPDiscordBot/windows-x64/scpdiscord.exe'
              env.BASIC_WINDOWS_SC_PATH = 'SCPDiscordBot/windows-x64/scpdiscord-sc.exe'
            }
          }
        }
        stage('Plugin')
        {
          environment
          {
            DOTNET_CLI_HOME = "/tmp/.dotnet-plugin"
          }
          steps
          {
            dir(path: 'SCPDiscordPlugin')
            {
              sh 'dotnet build --output ./bin'
              sh 'mkdir dependencies'
              sh 'mv bin/SCPDiscord.dll ./'
              sh 'mv bin/System.Memory.dll ./'
              sh 'mv bin/Google.Protobuf.dll ./'
              sh 'mv bin/Newtonsoft.Json.dll ./'
              sh 'zip -r dependencies.zip System.Memory.dll Google.Protobuf.dll Newtonsoft.Json.dll'
            }
            archiveArtifacts(artifacts: 'SCPDiscordPlugin/dependencies.zip', onlyIfSuccessful: true)
            archiveArtifacts(artifacts: 'SCPDiscordPlugin/SCPDiscord.dll', onlyIfSuccessful: true)
            script
            {
              env.PLUGIN_PATH = 'SCPDiscordPlugin/SCPDiscord.dll'
              env.DEPENDENCIES_PATH = 'SCPDiscordPlugin/dependencies.zip'
            }
          }
        }
        stage('RHEL')
        {
          agent { dockerfile { filename 'ci-utilities/docker/RHEL8.Dockerfile' } }
          environment{ DISTRO="rhel" }
          steps
          {
            script
            {
              common.build_rpm_package(env.DISTRO, "packaging/scpdiscord.spec", env.PACKAGE_NAME, env.RPMBUILD_ARGS)
              env.RHEL_RPM_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}-*.x86_64.rpm", returnStdout: true).trim()
              env.RHEL_RPM_PATH = "${env.DISTRO}/${env.RHEL_RPM_NAME}"
              env.RHEL_SRPM_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}-*.src.rpm", returnStdout: true).trim()
              env.RHEL_SRPM_PATH = "${env.DISTRO}/${env.RHEL_SRPM_NAME}"
            }
            stash(includes: "${env.RHEL_RPM_PATH}, ${env.RHEL_SRPM_PATH}", name: "${env.DISTRO}-rpm")
          }
        }
        stage('Fedora')
        {
          agent { dockerfile { filename 'ci-utilities/docker/Fedora42.Dockerfile' } }
          environment { DISTRO="fedora" }
          steps
          {
            script
            {
              common.build_rpm_package(env.DISTRO, "packaging/scpdiscord.spec", env.PACKAGE_NAME, env.RPMBUILD_ARGS)
              env.FEDORA_RPM_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}-*.x86_64.rpm", returnStdout: true).trim()
              env.FEDORA_RPM_PATH = "${env.DISTRO}/${env.FEDORA_RPM_NAME}"
              env.FEDORA_SRPM_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}-*.src.rpm", returnStdout: true).trim()
              env.FEDORA_SRPM_PATH = "${env.DISTRO}/${env.FEDORA_SRPM_NAME}"
            }
            stash(includes: "${env.FEDORA_RPM_PATH}, ${env.FEDORA_SRPM_PATH}", name: "${env.DISTRO}-rpm")
          }
        }
        stage('Debian')
        {
          agent
          {
            dockerfile { filename 'ci-utilities/docker/Debian12.Dockerfile' }
          }
          environment { DISTRO="debian"; PACKAGE_ROOT="${WORKSPACE}/debian" }
          steps
          {
            sh './packaging/generate-deb.sh'
            script
            {
              env.DEBIAN_DEB_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}_*_amd64.deb", returnStdout: true).trim()
              env.DEBIAN_DEB_PATH = "${env.DISTRO}/${env.DEBIAN_DEB_NAME}"
              env.DEBIAN_DSC_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}_*.dsc", returnStdout: true).trim()
              env.DEBIAN_DSC_PATH = "${env.DISTRO}/${env.DEBIAN_DSC_NAME}"
              env.DEBIAN_SRC_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}_*.tar.xz", returnStdout: true).trim()
              env.DEBIAN_SRC_PATH = "${env.DISTRO}/${env.DEBIAN_SRC_NAME}"
            }
            stash(includes: "${env.DEBIAN_DEB_PATH}, ${env.DEBIAN_SRC_PATH}, ${env.DEBIAN_DSC_PATH}", name: "${env.DISTRO}-deb")
          }
        }
        stage('Ubuntu')
        {
          agent
          {
            dockerfile { filename 'ci-utilities/docker/Ubuntu24.04.Dockerfile' }
          }
          environment { DISTRO="ubuntu"; PACKAGE_ROOT="${WORKSPACE}/ubuntu" }
          steps
          {
            sh './packaging/generate-deb.sh'
            script
            {
              env.UBUNTU_DEB_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}_*_amd64.deb", returnStdout: true).trim()
              env.UBUNTU_DEB_PATH = "${env.DISTRO}/${env.UBUNTU_DEB_NAME}"
              env.UBUNTU_DSC_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}_*.dsc", returnStdout: true).trim()
              env.UBUNTU_DSC_PATH = "${env.DISTRO}/${env.UBUNTU_DSC_NAME}"
              env.UBUNTU_SRC_NAME = sh(script: "cd ${env.DISTRO} && ls ${env.PACKAGE_NAME}_*.tar.xz", returnStdout: true).trim()
              env.UBUNTU_SRC_PATH = "${env.DISTRO}/${env.UBUNTU_SRC_NAME}"
            }
            stash(includes: "${env.UBUNTU_DEB_PATH}, ${env.UBUNTU_SRC_PATH}, ${env.UBUNTU_DSC_PATH}", name: "${env.DISTRO}-deb")
          }
        }
      }
    }
    stage('Sign')
    {
      parallel
      {
        stage('RHEL')
        {
          steps
          {
            unstash(name: 'rhel-rpm')
            script
            {
              common.sign_rpm_package(env.RHEL_RPM_PATH)
              common.sign_rpm_package(env.RHEL_SRPM_PATH)
            }
            archiveArtifacts(artifacts: "${env.RHEL_RPM_PATH}, ${env.RHEL_SRPM_PATH}", caseSensitive: true)
          }
        }
        stage('Fedora')
        {
          steps
          {
            unstash(name: 'fedora-rpm')
            script
            {
              common.sign_rpm_package(env.FEDORA_RPM_PATH)
              common.sign_rpm_package(env.FEDORA_SRPM_PATH)
            }
            archiveArtifacts(artifacts: "${env.FEDORA_RPM_PATH}, ${env.FEDORA_SRPM_PATH}", caseSensitive: true)
          }
        }
        stage('Debian')
        {
          steps
          {
            unstash(name: "debian-deb")
            script { common.sign_deb_package(env.DEBIAN_DEB_PATH, env.DEBIAN_DSC_PATH) }
            archiveArtifacts(artifacts: "${env.DEBIAN_DEB_PATH}, ${env.DEBIAN_SRC_PATH}", caseSensitive: true)
          }
        }
        stage('Ubuntu')
        {
          steps
          {
            unstash(name: "ubuntu-deb")
            script { common.sign_deb_package(env.UBUNTU_DEB_PATH, env.UBUNTU_DSC_PATH) }
            archiveArtifacts(artifacts: "${env.UBUNTU_DEB_PATH}, ${env.UBUNTU_SRC_PATH}", caseSensitive: true)
          }
        }
      }
    }
    stage('Deploy')
    {
      parallel
      {
        stage('RHEL')
        {
          when
          {
            expression { return env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'beta' || params.BUILD_TYPE != 'dev'; }
          }
          steps
          {
            script
            {
              common.publish_rpm_package("rhel/el8", env.RHEL_RPM_PATH, env.RHEL_SRPM_PATH, env.PACKAGE_NAME)
              common.publish_rpm_package("rhel/el9", env.RHEL_RPM_PATH, env.RHEL_SRPM_PATH, env.PACKAGE_NAME)
              common.publish_rpm_package("rhel/el10", env.RHEL_RPM_PATH, env.RHEL_SRPM_PATH, env.PACKAGE_NAME)
            }
          }
        }
        stage('Fedora')
        {
          when
          {
            expression { return env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'beta' || params.BUILD_TYPE != 'dev'; }
          }
          steps
          {
            script
            {
              common.publish_rpm_package("fedora", env.FEDORA_RPM_PATH, env.FEDORA_SRPM_PATH, env.PACKAGE_NAME)
            }
          }
        }
        stage('Debian')
        {
          when
          {
            expression { return env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'beta' || params.BUILD_TYPE != 'dev'; }
          }
          environment { DISTRO="debian"; COMPONENT="main" }
          steps
          {
            script
            {
              common.publish_deb_package(env.DISTRO, env.PACKAGE_NAME, env.PACKAGE_NAME, "${WORKSPACE}/${env.DISTRO}", env.COMPONENT)
              common.generate_debian_release_file("${WORKSPACE}/ci-utilities", env.DISTRO)
            }
          }
        }
        stage('Ubuntu')
        {
          when
          {
            expression { return env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'beta' || params.BUILD_TYPE != 'dev'; }
          }
          environment { DISTRO="ubuntu"; COMPONENT="main" }
          steps
          {
            script
            {
              common.publish_deb_package(env.DISTRO, env.PACKAGE_NAME, env.PACKAGE_NAME, "${WORKSPACE}/${env.DISTRO}", env.COMPONENT)
              common.generate_debian_release_file("${WORKSPACE}/ci-utilities", env.DISTRO)
            }
          }
        }
      }
    }
    stage('Release')
    {
      when
      {
        expression { params.BUILD_TYPE != 'dev'; }
      }
      steps
      {
        script
        {
          def artifacts = [
            env.BASIC_LINUX_PATH,
            env.BASIC_LINUX_SC_PATH,
            env.BASIC_WINDOWS_PATH,
            env.BASIC_WINDOWS_SC_PATH,
            env.PLUGIN_PATH,
            env.DEPENDENCIES_PATH,
            env.RHEL_RPM_PATH,
            //env.RHEL_SRPM_PATH,
            env.FEDORA_RPM_PATH,
            //env.FEDORA_SRPM_PATH,
            env.DEBIAN_DEB_PATH,
            //env.DEBIAN_SRC_PATH,
            env.UBUNTU_DEB_PATH,
            //env.UBUNTU_SRC_PATH
          ]

          currentBuild.description = params.BUILD_TYPE == 'pre-release' ? "Pre-release ${env.RELEASE_VERSION}" : "Release ${env.RELEASE_VERSION}"
          common.create_github_release("KarlOfDuty/SCPDiscord", params.RELEASE_VERSION, artifacts, params.BUILD_TYPE == 'pre-release')

          // Update AUR version after the tag is created
          common.update_aur_git_package(env.AUR_GIT_PACKAGE, "packaging/${env.AUR_GIT_PACKAGE}.pkgbuild", "packaging/scpdiscord.install")
        }
      }
    }
  }
}
